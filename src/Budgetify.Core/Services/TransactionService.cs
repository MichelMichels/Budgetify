using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request)
    {
        if (request.Amount == 0)
            throw new ArgumentException("Transaction amount cannot be zero.");

        var type = request.Type.ToUpperInvariant();
        if (type != "EXPENSE" && type != "INCOME" && type != "TRANSFER")
            throw new ArgumentException($"Invalid transaction type: {request.Type}. Allowed types: EXPENSE, INCOME, TRANSFER.");

        var sourceAccount = await _accountRepository.GetByIdAsync(request.AccountId)
            ?? throw new ArgumentException($"Source account with ID {request.AccountId} was not found.");

        if (!sourceAccount.IsActive)
            throw new InvalidOperationException("Cannot create transaction against an inactive account.");

        if (type == "EXPENSE" || type == "INCOME")
        {
            if (!request.CategoryId.HasValue)
                throw new ArgumentException($"Category ID is required for {type} transactions.");

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value)
                ?? throw new ArgumentException($"Category with ID {request.CategoryId.Value} was not found.");

            if (!category.FlowType.Equals(type, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Category flow type ({category.FlowType}) does not match transaction type ({type}).");
        }
        else if (type == "TRANSFER")
        {
            if (!request.DestinationAccountId.HasValue)
                throw new ArgumentException("Destination account ID is required for TRANSFER transactions.");

            if (request.DestinationAccountId.Value == request.AccountId)
                throw new ArgumentException("Destination account cannot be the same as the source account.");

            if (request.Amount <= 0)
                throw new ArgumentException("Transfer amount must be a positive number.");

            var destAccount = await _accountRepository.GetByIdAsync(request.DestinationAccountId.Value)
                ?? throw new ArgumentException($"Destination account with ID {request.DestinationAccountId.Value} was not found.");

            if (!destAccount.IsActive)
                throw new InvalidOperationException("Cannot transfer funds to an inactive destination account.");
        }

        var transaction = new Transaction
        {
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EUR" : request.Currency.ToUpperInvariant(),
            Type = type,
            AccountId = request.AccountId,
            DestinationAccountId = type == "TRANSFER" ? request.DestinationAccountId : null,
            CategoryId = (type == "EXPENSE" || type == "INCOME") ? request.CategoryId : request.CategoryId,
            PartyId = request.PartyId,
            PaymentMethod = request.PaymentMethod,
            IsRecurring = request.IsRecurring ?? false,
            TransactionDate = request.TransactionDate,
            Description = request.Description,
            Notes = request.Notes
        };

        var created = await _transactionRepository.AddAsync(transaction);
        await ApplyBalanceDeltaAsync(created, reverse: false);
        return created;
    }

    public async Task<Transaction?> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request)
    {
        var existing = await _transactionRepository.GetByIdAsync(id);
        if (existing == null)
            return null;

        // Revert old transaction's balance impact first
        await ApplyBalanceDeltaAsync(existing, reverse: true);

        try
        {
            if (request.Amount.HasValue)
            {
                if (request.Amount.Value == 0)
                    throw new ArgumentException("Transaction amount cannot be zero.");
                existing.Amount = request.Amount.Value;
            }

            if (!string.IsNullOrEmpty(request.Currency))
                existing.Currency = request.Currency.ToUpperInvariant();

            if (!string.IsNullOrEmpty(request.Type))
            {
                var type = request.Type.ToUpperInvariant();
                if (type != "EXPENSE" && type != "INCOME" && type != "TRANSFER")
                    throw new ArgumentException($"Invalid transaction type: {request.Type}.");
                existing.Type = type;
            }

            if (request.AccountId.HasValue)
                existing.AccountId = request.AccountId.Value;

            if (request.DestinationAccountId.HasValue)
                existing.DestinationAccountId = request.DestinationAccountId.Value;

            if (request.CategoryId.HasValue)
                existing.CategoryId = request.CategoryId.Value;

            if (request.PartyId.HasValue)
                existing.PartyId = request.PartyId.Value;

            if (request.PaymentMethod != null)
                existing.PaymentMethod = request.PaymentMethod;

            if (request.IsRecurring.HasValue)
                existing.IsRecurring = request.IsRecurring.Value;

            if (request.TransactionDate.HasValue)
                existing.TransactionDate = request.TransactionDate.Value;

            if (request.Description != null)
                existing.Description = request.Description;

            if (request.Notes != null)
                existing.Notes = request.Notes;

            // Re-validate updated state
            if (existing.Type == "EXPENSE" || existing.Type == "INCOME")
            {
                if (existing.CategoryId.HasValue)
                {
                    var cat = await _categoryRepository.GetByIdAsync(existing.CategoryId.Value);
                    if (cat != null && !cat.FlowType.Equals(existing.Type, StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException($"Category flow type ({cat.FlowType}) does not match transaction type ({existing.Type}).");
                }
            }
            else if (existing.Type == "TRANSFER")
            {
                if (!existing.DestinationAccountId.HasValue || existing.DestinationAccountId.Value == existing.AccountId)
                    throw new ArgumentException("Valid distinct destination account required for transfers.");
                if (existing.Amount <= 0)
                    throw new ArgumentException("Transfer amount must be positive.");
            }

            var updated = await _transactionRepository.UpdateAsync(existing);
            await ApplyBalanceDeltaAsync(existing, reverse: false);
            return updated;
        }
        catch
        {
            // Reapply old delta in case of error
            await ApplyBalanceDeltaAsync(existing, reverse: false);
            throw;
        }
    }

    public async Task<bool> DeleteTransactionAsync(Guid id)
    {
        var existing = await _transactionRepository.GetByIdAsync(id);
        if (existing == null)
            return false;

        await ApplyBalanceDeltaAsync(existing, reverse: true);
        return await _transactionRepository.DeleteAsync(id);
    }

    private async Task ApplyBalanceDeltaAsync(Transaction transaction, bool reverse)
    {
        var sign = reverse ? -1 : 1;
        var amount = transaction.Amount;

        switch (transaction.Type.ToUpperInvariant())
        {
            case "EXPENSE":
                // Standard expense (amount > 0) decreases balance (-amount).
                // Refund (amount < 0) increases balance (-(-amount) = +|amount|).
                await _accountRepository.UpdateBalanceAsync(transaction.AccountId, -sign * amount);
                break;

            case "INCOME":
                await _accountRepository.UpdateBalanceAsync(transaction.AccountId, sign * amount);
                break;

            case "TRANSFER":
                if (transaction.DestinationAccountId.HasValue)
                {
                    await _accountRepository.UpdateBalanceAsync(transaction.AccountId, -sign * amount);
                    await _accountRepository.UpdateBalanceAsync(transaction.DestinationAccountId.Value, sign * amount);
                }
                break;
        }
    }
}

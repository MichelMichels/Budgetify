using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Data;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly InMemoryDataStore _dataStore;

    public InMemoryTransactionRepository(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<Transaction>> GetAllAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        Guid? accountId = null,
        Guid? destinationAccountId = null,
        Guid? categoryId = null,
        Guid? partyId = null,
        string? type = null,
        string? currency = null,
        string? paymentMethod = null,
        bool? isRecurring = null,
        int limit = 50,
        int offset = 0)
    {
        var query = _dataStore.Transactions.Values.AsEnumerable();

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        if (destinationAccountId.HasValue)
            query = query.Where(t => t.DestinationAccountId == destinationAccountId.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (partyId.HasValue)
            query = query.Where(t => t.PartyId == partyId.Value);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(currency))
            query = query.Where(t => t.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(paymentMethod))
            query = query.Where(t => !string.IsNullOrEmpty(t.PaymentMethod) && t.PaymentMethod.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));

        if (isRecurring.HasValue)
            query = query.Where(t => t.IsRecurring == isRecurring.Value);

        var results = query
            .OrderByDescending(t => t.TransactionDate)
            .Skip(offset)
            .Take(limit > 0 ? limit : 50)
            .ToList();

        return Task.FromResult<IEnumerable<Transaction>>(results);
    }

    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        _dataStore.Transactions.TryGetValue(id, out var transaction);
        return Task.FromResult(transaction);
    }

    public Task<Transaction> AddAsync(Transaction transaction)
    {
        transaction.CreatedAt = DateTimeOffset.UtcNow;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Transactions[transaction.Id] = transaction;
        return Task.FromResult(transaction);
    }

    public Task<Transaction?> UpdateAsync(Transaction transaction)
    {
        if (!_dataStore.Transactions.ContainsKey(transaction.Id))
            return Task.FromResult<Transaction?>(null);

        transaction.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Transactions[transaction.Id] = transaction;
        return Task.FromResult<Transaction?>(transaction);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _dataStore.Transactions.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}

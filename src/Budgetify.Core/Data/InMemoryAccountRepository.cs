using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Data;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly InMemoryDataStore _dataStore;

    public InMemoryAccountRepository(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<Account>> GetAllAsync(string? accountType = null, bool? isActive = null, string? currency = null)
    {
        var query = _dataStore.Accounts.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(accountType))
            query = query.Where(a => a.AccountType.Equals(accountType, StringComparison.OrdinalIgnoreCase));

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(currency))
            query = query.Where(a => a.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IEnumerable<Account>>(query.OrderBy(a => a.Name).ToList());
    }

    public Task<Account?> GetByIdAsync(Guid id)
    {
        _dataStore.Accounts.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<Account> AddAsync(Account account)
    {
        account.CreatedAt = DateTimeOffset.UtcNow;
        account.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Accounts[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<Account?> UpdateAsync(Account account)
    {
        if (!_dataStore.Accounts.ContainsKey(account.Id))
            return Task.FromResult<Account?>(null);

        account.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Accounts[account.Id] = account;
        return Task.FromResult<Account?>(account);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _dataStore.Accounts.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> HasAssociatedTransactionsAsync(Guid id)
    {
        var hasTransactions = _dataStore.Transactions.Values.Any(t =>
            t.AccountId == id || t.DestinationAccountId == id);
        return Task.FromResult(hasTransactions);
    }

    public Task UpdateBalanceAsync(Guid accountId, long delta)
    {
        if (_dataStore.Accounts.TryGetValue(accountId, out var account))
        {
            account.CurrentBalance += delta;
            account.UpdatedAt = DateTimeOffset.UtcNow;
        }
        return Task.CompletedTask;
    }
}

using Budgetify.Core.Entities;

namespace Budgetify.Core.Interfaces;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync(string? accountType = null, bool? isActive = null, string? currency = null);
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account> AddAsync(Account account);
    Task<Account?> UpdateAsync(Account account);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HasAssociatedTransactionsAsync(Guid id);
    Task UpdateBalanceAsync(Guid accountId, long delta);
}

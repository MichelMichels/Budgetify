using Budgetify.Core.Entities;

namespace Budgetify.Core.Interfaces;

public interface IPartyRepository
{
    Task<IEnumerable<Party>> GetAllAsync(string? partyType = null, bool? isActive = null, string? search = null);
    Task<Party?> GetByIdAsync(Guid id);
    Task<Party> AddAsync(Party party);
    Task<Party?> UpdateAsync(Party party);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HasAssociatedTransactionsAsync(Guid id);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null);
}

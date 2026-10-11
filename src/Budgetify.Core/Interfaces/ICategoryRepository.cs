using Budgetify.Core.Entities;

namespace Budgetify.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(string? flowType = null, bool? isActive = null);
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category> AddAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HasAssociatedTransactionsAsync(Guid id);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null);
}

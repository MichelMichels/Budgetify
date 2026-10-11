using Budgetify.Core.Entities;

namespace Budgetify.Core.Interfaces;

public interface IBudgetRepository
{
    Task<IEnumerable<Budget>> GetAllAsync(
        Guid? categoryId = null,
        string? period = null,
        bool? isRecurring = null,
        string? currency = null
    );
    Task<Budget?> GetByIdAsync(Guid id);
    Task<Budget> AddAsync(Budget budget);
    Task<Budget?> UpdateAsync(Budget budget);
    Task<bool> DeleteAsync(Guid id);
    Task<Budget?> GetForCategoryAndPeriodAsync(Guid categoryId, string? period);
}

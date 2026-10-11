using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Data;

public class InMemoryBudgetRepository : IBudgetRepository
{
    private readonly InMemoryDataStore _dataStore;

    public InMemoryBudgetRepository(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<Budget>> GetAllAsync(
        Guid? categoryId = null,
        string? period = null,
        bool? isRecurring = null,
        string? currency = null)
    {
        var query = _dataStore.Budgets.Values.AsEnumerable();

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(period))
        {
            if (period.Equals("recurring", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => string.IsNullOrEmpty(b.Period));
            else
                query = query.Where(b => !string.IsNullOrEmpty(b.Period) && b.Period.Equals(period, StringComparison.OrdinalIgnoreCase));
        }

        if (isRecurring.HasValue)
            query = query.Where(b => b.IsRecurring == isRecurring.Value);

        if (!string.IsNullOrEmpty(currency))
            query = query.Where(b => b.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IEnumerable<Budget>>(query.OrderBy(b => b.Period).ToList());
    }

    public Task<Budget?> GetByIdAsync(Guid id)
    {
        _dataStore.Budgets.TryGetValue(id, out var budget);
        return Task.FromResult(budget);
    }

    public Task<Budget> AddAsync(Budget budget)
    {
        budget.CreatedAt = DateTimeOffset.UtcNow;
        budget.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Budgets[budget.Id] = budget;
        return Task.FromResult(budget);
    }

    public Task<Budget?> UpdateAsync(Budget budget)
    {
        if (!_dataStore.Budgets.ContainsKey(budget.Id))
            return Task.FromResult<Budget?>(null);

        budget.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Budgets[budget.Id] = budget;
        return Task.FromResult<Budget?>(budget);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _dataStore.Budgets.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<Budget?> GetForCategoryAndPeriodAsync(Guid categoryId, string? period)
    {
        var budget = _dataStore.Budgets.Values.FirstOrDefault(b =>
            b.CategoryId == categoryId &&
            ((string.IsNullOrEmpty(period) && string.IsNullOrEmpty(b.Period)) ||
             (!string.IsNullOrEmpty(period) && !string.IsNullOrEmpty(b.Period) && b.Period.Equals(period, StringComparison.OrdinalIgnoreCase))));

        return Task.FromResult(budget);
    }
}

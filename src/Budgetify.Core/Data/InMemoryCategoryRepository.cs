using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Data;

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly InMemoryDataStore _dataStore;

    public InMemoryCategoryRepository(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<Category>> GetAllAsync(string? flowType = null, bool? isActive = null)
    {
        var query = _dataStore.Categories.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(flowType))
            query = query.Where(c => c.FlowType.Equals(flowType, StringComparison.OrdinalIgnoreCase));

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        return Task.FromResult<IEnumerable<Category>>(query.OrderBy(c => c.Description).ToList());
    }

    public Task<Category?> GetByIdAsync(Guid id)
    {
        _dataStore.Categories.TryGetValue(id, out var category);
        return Task.FromResult(category);
    }

    public Task<Category> AddAsync(Category category)
    {
        category.CreatedAt = DateTimeOffset.UtcNow;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Categories[category.Id] = category;
        return Task.FromResult(category);
    }

    public Task<Category?> UpdateAsync(Category category)
    {
        if (!_dataStore.Categories.ContainsKey(category.Id))
            return Task.FromResult<Category?>(null);

        category.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Categories[category.Id] = category;
        return Task.FromResult<Category?>(category);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _dataStore.Categories.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> HasAssociatedTransactionsAsync(Guid id)
    {
        var hasTransactions = _dataStore.Transactions.Values.Any(t => t.CategoryId == id);
        return Task.FromResult(hasTransactions);
    }

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null)
    {
        var exists = _dataStore.Categories.Values.Any(c =>
            c.CategoryCode.Equals(code, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || c.Id != excludeId.Value));
        return Task.FromResult(exists);
    }
}

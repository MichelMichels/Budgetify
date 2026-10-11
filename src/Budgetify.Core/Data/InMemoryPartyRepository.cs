using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Data;

public class InMemoryPartyRepository : IPartyRepository
{
    private readonly InMemoryDataStore _dataStore;

    public InMemoryPartyRepository(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<Party>> GetAllAsync(string? partyType = null, bool? isActive = null, string? search = null)
    {
        var query = _dataStore.Parties.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(partyType))
            query = query.Where(p => p.PartyType.Equals(partyType, StringComparison.OrdinalIgnoreCase));

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.PartyCode.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IEnumerable<Party>>(query.OrderBy(p => p.Name).ToList());
    }

    public Task<Party?> GetByIdAsync(Guid id)
    {
        _dataStore.Parties.TryGetValue(id, out var party);
        return Task.FromResult(party);
    }

    public Task<Party> AddAsync(Party party)
    {
        party.CreatedAt = DateTimeOffset.UtcNow;
        party.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Parties[party.Id] = party;
        return Task.FromResult(party);
    }

    public Task<Party?> UpdateAsync(Party party)
    {
        if (!_dataStore.Parties.ContainsKey(party.Id))
            return Task.FromResult<Party?>(null);

        party.UpdatedAt = DateTimeOffset.UtcNow;
        _dataStore.Parties[party.Id] = party;
        return Task.FromResult<Party?>(party);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _dataStore.Parties.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> HasAssociatedTransactionsAsync(Guid id)
    {
        var hasTransactions = _dataStore.Transactions.Values.Any(t => t.PartyId == id);
        return Task.FromResult(hasTransactions);
    }

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null)
    {
        var exists = _dataStore.Parties.Values.Any(p =>
            p.PartyCode.Equals(code, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || p.Id != excludeId.Value));
        return Task.FromResult(exists);
    }
}

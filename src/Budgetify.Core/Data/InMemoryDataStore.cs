using System.Collections.Concurrent;
using Budgetify.Core.Entities;

namespace Budgetify.Core.Data;

public class InMemoryDataStore
{
    public ConcurrentDictionary<Guid, Category> Categories { get; } = new();
    public ConcurrentDictionary<Guid, Party> Parties { get; } = new();
    public ConcurrentDictionary<Guid, Account> Accounts { get; } = new();
    public ConcurrentDictionary<Guid, Transaction> Transactions { get; } = new();
    public ConcurrentDictionary<Guid, Budget> Budgets { get; } = new();

    public InMemoryDataStore()
    {
        // Optional default seed data can be placed here if desired
    }
}

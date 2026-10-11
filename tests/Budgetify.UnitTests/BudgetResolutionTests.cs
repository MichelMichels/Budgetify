using Budgetify.Core.Data;
using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Services;

namespace Budgetify.UnitTests;

public class BudgetResolutionTests
{
    private readonly InMemoryDataStore _dataStore;
    private readonly InMemoryTransactionRepository _txRepo;
    private readonly InMemoryAccountRepository _accRepo;
    private readonly InMemoryCategoryRepository _catRepo;
    private readonly InMemoryPartyRepository _partyRepo;
    private readonly InMemoryBudgetRepository _budgetRepo;
    private readonly ReportService _reportService;

    public BudgetResolutionTests()
    {
        _dataStore = new InMemoryDataStore();
        _txRepo = new InMemoryTransactionRepository(_dataStore);
        _accRepo = new InMemoryAccountRepository(_dataStore);
        _catRepo = new InMemoryCategoryRepository(_dataStore);
        _partyRepo = new InMemoryPartyRepository(_dataStore);
        _budgetRepo = new InMemoryBudgetRepository(_dataStore);
        _reportService = new ReportService(_txRepo, _catRepo, _partyRepo, _accRepo, _budgetRepo);
    }

    [Fact]
    public async Task MonthSpecificOverride_TakesPrecedenceOver_RecurringBaseline()
    {
        // Arrange
        var category = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 100000 });

        // Baseline: €500/month
        await _budgetRepo.AddAsync(new Budget
        {
            CategoryId = category.Id,
            Period = null,
            Amount = 50000,
            Currency = "EUR"
        });

        // December override: €800
        await _budgetRepo.AddAsync(new Budget
        {
            CategoryId = category.Id,
            Period = "2023-12",
            Amount = 80000,
            Currency = "EUR"
        });

        // Add a December transaction: €600
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 60000,
            Currency = "EUR",
            Type = "EXPENSE",
            AccountId = account.Id,
            CategoryId = category.Id,
            TransactionDate = new DateTimeOffset(2023, 12, 15, 12, 0, 0, TimeSpan.Zero),
            Description = "Holiday groceries"
        });

        // Act
        var report = await _reportService.GetBudgetVarianceAsync("2023-12", "EUR");

        // Assert
        Assert.Single(report.Categories);
        var item = report.Categories[0];
        Assert.Equal(80000, item.BudgetAmount); // Resolved the 80000 override!
        Assert.Equal(60000, item.ActualAmount);
        Assert.Equal(20000, item.VarianceAmount); // 80000 - 60000 = 20000 under budget
        Assert.False(item.IsOverBudget);
        Assert.True(item.IsOverride);
        Assert.False(item.IsRecurring);
    }

    [Fact]
    public async Task FallbackTo_RecurringBaseline_WhenNoMonthOverrideExists()
    {
        // Arrange
        var category = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 100000 });

        // Baseline only: €500/month
        await _budgetRepo.AddAsync(new Budget
        {
            CategoryId = category.Id,
            Period = null,
            Amount = 50000,
            Currency = "EUR"
        });

        // Transaction in March 2023: €550 (over budget)
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 55000,
            Currency = "EUR",
            Type = "EXPENSE",
            AccountId = account.Id,
            CategoryId = category.Id,
            TransactionDate = new DateTimeOffset(2023, 3, 20, 10, 0, 0, TimeSpan.Zero),
            Description = "March groceries"
        });

        // Act
        var report = await _reportService.GetBudgetVarianceAsync("2023-03", "EUR");

        // Assert
        Assert.Single(report.Categories);
        var item = report.Categories[0];
        Assert.Equal(50000, item.BudgetAmount); // Fell back to recurring baseline of 50000!
        Assert.Equal(55000, item.ActualAmount);
        Assert.Equal(-5000, item.VarianceAmount); // Over budget by 5000
        Assert.True(item.IsOverBudget);
        Assert.False(item.IsOverride);
        Assert.True(item.IsRecurring);
    }
}

using Budgetify.Core.Data;
using Budgetify.Core.Entities;
using Budgetify.Core.Services;

namespace Budgetify.UnitTests;

public class ReportServiceTests
{
    private readonly InMemoryDataStore _dataStore;
    private readonly InMemoryTransactionRepository _txRepo;
    private readonly InMemoryAccountRepository _accRepo;
    private readonly InMemoryCategoryRepository _catRepo;
    private readonly InMemoryPartyRepository _partyRepo;
    private readonly InMemoryBudgetRepository _budgetRepo;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _dataStore = new InMemoryDataStore();
        _txRepo = new InMemoryTransactionRepository(_dataStore);
        _accRepo = new InMemoryAccountRepository(_dataStore);
        _catRepo = new InMemoryCategoryRepository(_dataStore);
        _partyRepo = new InMemoryPartyRepository(_dataStore);
        _budgetRepo = new InMemoryBudgetRepository(_dataStore);
        _sut = new ReportService(_txRepo, _catRepo, _partyRepo, _accRepo, _budgetRepo);
    }

    [Fact]
    public async Task Transfers_AreExcludedFrom_ExpensesAndIncome()
    {
        // Arrange
        var checking = await _accRepo.AddAsync(new Account { Name = "Checking", AccountType = "CHECKING", CurrentBalance = 100000 });
        var savings = await _accRepo.AddAsync(new Account { Name = "Savings", AccountType = "SAVINGS", CurrentBalance = 50000 });
        var expenseCat = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });
        var incomeCat = await _catRepo.AddAsync(new Category { Description = "Salary", FlowType = "INCOME" });

        var date = new DateTimeOffset(2023, 1, 15, 12, 0, 0, TimeSpan.Zero);

        // 1. Income: €3000
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 300000,
            Currency = "EUR",
            Type = "INCOME",
            AccountId = checking.Id,
            CategoryId = incomeCat.Id,
            TransactionDate = date,
            Description = "Salary"
        });

        // 2. Expense: €800
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 80000,
            Currency = "EUR",
            Type = "EXPENSE",
            AccountId = checking.Id,
            CategoryId = expenseCat.Id,
            TransactionDate = date,
            Description = "Groceries"
        });

        // 3. Internal Transfer: €500 (Checking -> Savings)
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 50000,
            Currency = "EUR",
            Type = "TRANSFER",
            AccountId = checking.Id,
            DestinationAccountId = savings.Id,
            TransactionDate = date,
            Description = "Transfer to Savings"
        });

        // Act
        var summary = await _sut.GetExpenseSummaryAsync(
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 31, 23, 59, 59, TimeSpan.Zero),
            "EUR"
        );

        // Assert
        Assert.Equal(80000, summary.TotalExpenses); // €800 only, transfer is excluded!
        Assert.Equal(300000, summary.TotalIncome);   // €3000 only
        Assert.Equal(220000, summary.NetCashFlow);   // 300000 - 80000 = 220000
        Assert.Equal(50000, summary.TotalTransfersToSavings); // Tracked under savings transfers!
        Assert.Equal(16.67m, summary.SavingsRatePercentage); // (50000 / 300000) * 100 = 16.67%
    }

    [Fact]
    public async Task RecurringVsVariableExpenses_CalculatesAccurateSplit()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 100000 });
        var category = await _catRepo.AddAsync(new Category { Description = "Expenses", FlowType = "EXPENSE" });
        var date = new DateTimeOffset(2023, 1, 15, 12, 0, 0, TimeSpan.Zero);

        // Recurring: €600 (Rent)
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 60000,
            Currency = "EUR",
            Type = "EXPENSE",
            AccountId = account.Id,
            CategoryId = category.Id,
            IsRecurring = true,
            TransactionDate = date,
            Description = "Rent"
        });

        // Variable: €400 (Dining)
        await _txRepo.AddAsync(new Transaction
        {
            Amount = 40000,
            Currency = "EUR",
            Type = "EXPENSE",
            AccountId = account.Id,
            CategoryId = category.Id,
            IsRecurring = false,
            TransactionDate = date,
            Description = "Dining"
        });

        // Act
        var report = await _sut.GetRecurringExpensesAsync(
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 31, 23, 59, 59, TimeSpan.Zero),
            "EUR"
        );

        // Assert
        Assert.Equal(100000, report.TotalExpenses);
        Assert.Equal(60000, report.Recurring.Amount);
        Assert.Equal(60.00m, report.Recurring.Percentage);
        Assert.Equal(40000, report.Variable.Amount);
        Assert.Equal(40.00m, report.Variable.Percentage);
    }
}

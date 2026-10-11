using Budgetify.Core.Data;
using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Services;

namespace Budgetify.UnitTests;

public class TransactionServiceTests
{
    private readonly InMemoryDataStore _dataStore;
    private readonly InMemoryTransactionRepository _txRepo;
    private readonly InMemoryAccountRepository _accRepo;
    private readonly InMemoryCategoryRepository _catRepo;
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        _dataStore = new InMemoryDataStore();
        _txRepo = new InMemoryTransactionRepository(_dataStore);
        _accRepo = new InMemoryAccountRepository(_dataStore);
        _catRepo = new InMemoryCategoryRepository(_dataStore);
        _sut = new TransactionService(_txRepo, _accRepo, _catRepo);
    }

    [Fact]
    public async Task CreateExpense_DecreasesAccountBalance()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 10000 });
        var category = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });

        var request = new CreateTransactionRequest(
            Amount: 2500, // €25.00
            Currency: "EUR",
            Type: "EXPENSE",
            AccountId: account.Id,
            DestinationAccountId: null,
            CategoryId: category.Id,
            PartyId: null,
            PaymentMethod: "DEBIT_CARD",
            IsRecurring: false,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Supermarket",
            Notes: null
        );

        // Act
        var result = await _sut.CreateTransactionAsync(request);

        // Assert
        Assert.NotNull(result);
        var updatedAccount = await _accRepo.GetByIdAsync(account.Id);
        Assert.Equal(7500, updatedAccount!.CurrentBalance); // 10000 - 2500 = 7500
    }

    [Fact]
    public async Task CreateRefund_NegativeExpense_IncreasesAccountBalance()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 5000 });
        var category = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });

        var refundRequest = new CreateTransactionRequest(
            Amount: -1500, // -€15.00 refund
            Currency: "EUR",
            Type: "EXPENSE",
            AccountId: account.Id,
            DestinationAccountId: null,
            CategoryId: category.Id,
            PartyId: null,
            PaymentMethod: "DEBIT_CARD",
            IsRecurring: false,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Returned item",
            Notes: null
        );

        // Act
        var result = await _sut.CreateTransactionAsync(refundRequest);

        // Assert
        Assert.NotNull(result);
        var updatedAccount = await _accRepo.GetByIdAsync(account.Id);
        Assert.Equal(6500, updatedAccount!.CurrentBalance); // 5000 - (-1500) = 6500
    }

    [Fact]
    public async Task CreateIncome_IncreasesAccountBalance()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 2000 });
        var category = await _catRepo.AddAsync(new Category { Description = "Salary", FlowType = "INCOME" });

        var request = new CreateTransactionRequest(
            Amount: 300000, // €3000.00
            Currency: "EUR",
            Type: "INCOME",
            AccountId: account.Id,
            DestinationAccountId: null,
            CategoryId: category.Id,
            PartyId: null,
            PaymentMethod: "BANK_TRANSFER",
            IsRecurring: true,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Monthly salary",
            Notes: null
        );

        // Act
        var result = await _sut.CreateTransactionAsync(request);

        // Assert
        Assert.NotNull(result);
        var updatedAccount = await _accRepo.GetByIdAsync(account.Id);
        Assert.Equal(302000, updatedAccount!.CurrentBalance); // 2000 + 300000
    }

    [Fact]
    public async Task CreateTransfer_MovesFundsBetweenAccounts()
    {
        // Arrange
        var sourceAcc = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 50000 });
        var destAcc = await _accRepo.AddAsync(new Account { Name = "Savings", CurrentBalance = 10000, AccountType = "SAVINGS" });

        var request = new CreateTransactionRequest(
            Amount: 20000, // €200.00
            Currency: "EUR",
            Type: "TRANSFER",
            AccountId: sourceAcc.Id,
            DestinationAccountId: destAcc.Id,
            CategoryId: null,
            PartyId: null,
            PaymentMethod: "BANK_TRANSFER",
            IsRecurring: false,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Savings transfer",
            Notes: null
        );

        // Act
        var result = await _sut.CreateTransactionAsync(request);

        // Assert
        Assert.NotNull(result);
        var updatedSource = await _accRepo.GetByIdAsync(sourceAcc.Id);
        var updatedDest = await _accRepo.GetByIdAsync(destAcc.Id);
        Assert.Equal(30000, updatedSource!.CurrentBalance); // 50000 - 20000
        Assert.Equal(30000, updatedDest!.CurrentBalance);   // 10000 + 20000
    }

    [Fact]
    public async Task CreateExpense_WithIncomeCategory_ThrowsArgumentException()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 10000 });
        var incomeCategory = await _catRepo.AddAsync(new Category { Description = "Salary", FlowType = "INCOME" });

        var request = new CreateTransactionRequest(
            Amount: 1000,
            Currency: "EUR",
            Type: "EXPENSE",
            AccountId: account.Id,
            DestinationAccountId: null,
            CategoryId: incomeCategory.Id,
            PartyId: null,
            PaymentMethod: "DEBIT_CARD",
            IsRecurring: false,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Invalid expense",
            Notes: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateTransactionAsync(request));
    }

    [Fact]
    public async Task CreateTransaction_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 10000 });
        var category = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });

        var request = new CreateTransactionRequest(
            Amount: 0,
            Currency: "EUR",
            Type: "EXPENSE",
            AccountId: account.Id,
            DestinationAccountId: null,
            CategoryId: category.Id,
            PartyId: null,
            PaymentMethod: "DEBIT_CARD",
            IsRecurring: false,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Zero amount",
            Notes: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateTransactionAsync(request));
    }

    [Fact]
    public async Task DeleteTransaction_ReversesBalanceImpact()
    {
        // Arrange
        var account = await _accRepo.AddAsync(new Account { Name = "Checking", CurrentBalance = 10000 });
        var category = await _catRepo.AddAsync(new Category { Description = "Groceries", FlowType = "EXPENSE" });

        var request = new CreateTransactionRequest(
            Amount: 4000,
            Currency: "EUR",
            Type: "EXPENSE",
            AccountId: account.Id,
            DestinationAccountId: null,
            CategoryId: category.Id,
            PartyId: null,
            PaymentMethod: "DEBIT_CARD",
            IsRecurring: false,
            TransactionDate: DateTimeOffset.UtcNow,
            Description: "Groceries",
            Notes: null
        );

        var created = await _sut.CreateTransactionAsync(request);
        Assert.Equal(6000, (await _accRepo.GetByIdAsync(account.Id))!.CurrentBalance);

        // Act
        var deleted = await _sut.DeleteTransactionAsync(created.Id);

        // Assert
        Assert.True(deleted);
        var restoredAccount = await _accRepo.GetByIdAsync(account.Id);
        Assert.Equal(10000, restoredAccount!.CurrentBalance); // Restored back to 10000
    }
}

namespace Budgetify.Core.DTOs;

public record ExpenseSummaryReport(
    string Currency,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    long TotalExpenses,
    long TotalIncome,
    long NetCashFlow,
    long TotalTransfersToSavings,
    decimal SavingsRatePercentage,
    int TransactionCount,
    long AverageExpenseAmount,
    List<ExpenseTrendItem> Trend
);

public record ExpenseTrendItem(
    string Period,
    long Expenses,
    long Income,
    long Net,
    long SavingsTransfers,
    int TransactionCount
);

public record CategoryBreakdownReport(
    string Currency,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    long TotalSpend,
    List<CategorySpendItem> Categories
);

public record CategorySpendItem(
    Guid CategoryId,
    string CategoryName,
    long Amount,
    decimal PercentageOfTotal,
    int TransactionCount
);

public record PartySpendReport(
    string Currency,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    long TotalPartySpend,
    List<PartySpendItem> Parties
);

public record PartySpendItem(
    Guid PartyId,
    string PartyName,
    string PartyType,
    long TotalAmount,
    decimal PercentageOfSpend,
    int TransactionCount
);

public record BudgetVarianceReport(
    string Period,
    string Currency,
    long TotalBudgeted,
    long TotalActual,
    long TotalVariance,
    decimal OverallUtilizationPercentage,
    List<CategoryBudgetVarianceItem> Categories
);

public record CategoryBudgetVarianceItem(
    Guid CategoryId,
    string CategoryName,
    long? BudgetAmount,
    long ActualAmount,
    long? VarianceAmount,
    decimal? UtilizationPercentage,
    bool IsOverBudget,
    bool IsRecurring,
    bool IsOverride
);

public record RecurringExpensesReport(
    string Currency,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    long TotalExpenses,
    RecurringExpenseBreakdown Recurring,
    RecurringExpenseBreakdown Variable
);

public record RecurringExpenseBreakdown(
    long Amount,
    decimal Percentage,
    int TransactionCount
);

public record AccountBalancesReport(
    string Currency,
    long NetWorth,
    long LiquidAssets,
    List<AccountBalanceItem> Accounts
);

public record AccountBalanceItem(
    Guid AccountId,
    string AccountName,
    string AccountType,
    long CurrentBalance
);

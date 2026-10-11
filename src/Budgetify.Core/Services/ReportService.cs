using System.Text;
using Budgetify.Core.DTOs;
using Budgetify.Core.Interfaces;

namespace Budgetify.Core.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPartyRepository _partyRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBudgetRepository _budgetRepository;

    public ReportService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IPartyRepository partyRepository,
        IAccountRepository accountRepository,
        IBudgetRepository budgetRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _partyRepository = partyRepository;
        _accountRepository = accountRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task<ExpenseSummaryReport> GetExpenseSummaryAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR", string groupBy = "month")
    {
        var allTx = (await _transactionRepository.GetAllAsync(startDate: startDate, endDate: endDate, currency: currency, limit: int.MaxValue)).ToList();

        var expenses = allTx.Where(t => t.Type.Equals("EXPENSE", StringComparison.OrdinalIgnoreCase)).ToList();
        var income = allTx.Where(t => t.Type.Equals("INCOME", StringComparison.OrdinalIgnoreCase)).ToList();
        var transfers = allTx.Where(t => t.Type.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase)).ToList();

        long totalExpenses = expenses.Sum(t => t.Amount);
        long totalIncome = income.Sum(t => t.Amount);
        long netCashFlow = totalIncome - totalExpenses;

        // Calculate transfers to savings
        long totalTransfersToSavings = 0;
        var accounts = (await _accountRepository.GetAllAsync(currency: currency)).ToDictionary(a => a.Id);
        foreach (var transfer in transfers)
        {
            if (transfer.DestinationAccountId.HasValue &&
                accounts.TryGetValue(transfer.DestinationAccountId.Value, out var destAcc) &&
                destAcc.AccountType.Equals("SAVINGS", StringComparison.OrdinalIgnoreCase))
            {
                totalTransfersToSavings += transfer.Amount;
            }
        }

        decimal savingsRate = totalIncome > 0
            ? Math.Round(((decimal)totalTransfersToSavings / totalIncome) * 100m, 2)
            : 0m;

        int expenseCount = expenses.Count;
        long averageExpense = expenseCount > 0 ? totalExpenses / expenseCount : 0;

        // Group trend
        var trend = allTx
            .GroupBy(t => t.TransactionDate.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                long exp = g.Where(t => t.Type.Equals("EXPENSE", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
                long inc = g.Where(t => t.Type.Equals("INCOME", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
                long sav = g.Where(t => t.Type.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase) &&
                                        t.DestinationAccountId.HasValue &&
                                        accounts.TryGetValue(t.DestinationAccountId.Value, out var a) &&
                                        a.AccountType.Equals("SAVINGS", StringComparison.OrdinalIgnoreCase))
                            .Sum(t => t.Amount);
                return new ExpenseTrendItem(
                    Period: g.Key,
                    Expenses: exp,
                    Income: inc,
                    Net: inc - exp,
                    SavingsTransfers: sav,
                    TransactionCount: g.Count()
                );
            })
            .ToList();

        return new ExpenseSummaryReport(
            Currency: currency,
            StartDate: startDate,
            EndDate: endDate,
            TotalExpenses: totalExpenses,
            TotalIncome: totalIncome,
            NetCashFlow: netCashFlow,
            TotalTransfersToSavings: totalTransfersToSavings,
            SavingsRatePercentage: savingsRate,
            TransactionCount: allTx.Count,
            AverageExpenseAmount: averageExpense,
            Trend: trend
        );
    }

    public async Task<CategoryBreakdownReport> GetCategoryBreakdownAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR", string flowType = "EXPENSE")
    {
        var transactions = (await _transactionRepository.GetAllAsync(
            startDate: startDate,
            endDate: endDate,
            type: flowType,
            currency: currency,
            limit: int.MaxValue)).ToList();

        long totalSpend = transactions.Sum(t => t.Amount);
        var categories = (await _categoryRepository.GetAllAsync(flowType: flowType)).ToDictionary(c => c.Id);

        var grouped = transactions
            .Where(t => t.CategoryId.HasValue)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g =>
            {
                var categoryName = categories.TryGetValue(g.Key, out var cat) ? cat.Description : "Unknown Category";
                long amount = g.Sum(t => t.Amount);
                decimal percentage = totalSpend > 0 ? Math.Round(((decimal)amount / totalSpend) * 100m, 2) : 0m;
                return new CategorySpendItem(
                    CategoryId: g.Key,
                    CategoryName: categoryName,
                    Amount: amount,
                    PercentageOfTotal: percentage,
                    TransactionCount: g.Count()
                );
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        return new CategoryBreakdownReport(
            Currency: currency,
            StartDate: startDate,
            EndDate: endDate,
            TotalSpend: totalSpend,
            Categories: grouped
        );
    }

    public async Task<PartySpendReport> GetPartySpendAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR", string? partyType = null, int limit = 10)
    {
        var transactions = (await _transactionRepository.GetAllAsync(
            startDate: startDate,
            endDate: endDate,
            type: "EXPENSE",
            currency: currency,
            limit: int.MaxValue))
            .Where(t => t.PartyId.HasValue)
            .ToList();

        var parties = (await _partyRepository.GetAllAsync(partyType: partyType)).ToDictionary(p => p.Id);

        var partyTransactions = transactions
            .Where(t => parties.ContainsKey(t.PartyId!.Value))
            .ToList();

        long totalPartySpend = partyTransactions.Sum(t => t.Amount);

        var grouped = partyTransactions
            .GroupBy(t => t.PartyId!.Value)
            .Select(g =>
            {
                var party = parties[g.Key];
                long amount = g.Sum(t => t.Amount);
                decimal percentage = totalPartySpend > 0 ? Math.Round(((decimal)amount / totalPartySpend) * 100m, 2) : 0m;
                return new PartySpendItem(
                    PartyId: party.Id,
                    PartyName: party.Name,
                    PartyType: party.PartyType,
                    TotalAmount: amount,
                    PercentageOfSpend: percentage,
                    TransactionCount: g.Count()
                );
            })
            .OrderByDescending(p => p.TotalAmount)
            .Take(limit > 0 ? limit : 10)
            .ToList();

        return new PartySpendReport(
            Currency: currency,
            StartDate: startDate,
            EndDate: endDate,
            TotalPartySpend: totalPartySpend,
            Parties: grouped
        );
    }

    public async Task<BudgetVarianceReport> GetBudgetVarianceAsync(string period, string currency = "EUR")
    {
        // Parse period year-month
        if (!DateTime.TryParse($"{period}-01", out var periodDate))
            throw new ArgumentException("Period must be in YYYY-MM format.");

        var startDate = new DateTimeOffset(periodDate, TimeSpan.Zero);
        var endDate = new DateTimeOffset(periodDate.AddMonths(1).AddTicks(-1), TimeSpan.Zero);

        // Fetch all transactions for this month
        var transactions = (await _transactionRepository.GetAllAsync(
            startDate: startDate,
            endDate: endDate,
            type: "EXPENSE",
            currency: currency,
            limit: int.MaxValue)).ToList();

        var categories = (await _categoryRepository.GetAllAsync(flowType: "EXPENSE")).ToList();
        var allBudgets = (await _budgetRepository.GetAllAsync(currency: currency)).ToList();

        var categoryReports = new List<CategoryBudgetVarianceItem>();
        long totalBudgeted = 0;
        long totalActual = 0;

        foreach (var category in categories)
        {
            // 1. Look for specific month override
            var specificBudget = allBudgets.FirstOrDefault(b => b.CategoryId == category.Id && b.Period == period);
            // 2. Look for perpetual baseline (period is null)
            var recurringBudget = allBudgets.FirstOrDefault(b => b.CategoryId == category.Id && string.IsNullOrEmpty(b.Period));

            var effectiveBudget = specificBudget ?? recurringBudget;
            long actualAmount = transactions.Where(t => t.CategoryId == category.Id).Sum(t => t.Amount);

            if (effectiveBudget == null && actualAmount == 0)
                continue; // Skip categories with neither budget nor transactions

            long? budgetAmount = effectiveBudget?.Amount;
            long? varianceAmount = budgetAmount.HasValue ? budgetAmount.Value - actualAmount : null;
            decimal? utilization = budgetAmount.HasValue && budgetAmount.Value > 0
                ? Math.Round(((decimal)actualAmount / budgetAmount.Value) * 100m, 2)
                : null;
            bool isOver = budgetAmount.HasValue && actualAmount > budgetAmount.Value;

            categoryReports.Add(new CategoryBudgetVarianceItem(
                CategoryId: category.Id,
                CategoryName: category.Description,
                BudgetAmount: budgetAmount,
                ActualAmount: actualAmount,
                VarianceAmount: varianceAmount,
                UtilizationPercentage: utilization,
                IsOverBudget: isOver,
                IsRecurring: effectiveBudget?.IsRecurring ?? false,
                IsOverride: specificBudget != null
            ));

            if (budgetAmount.HasValue)
                totalBudgeted += budgetAmount.Value;
            totalActual += actualAmount;
        }

        long totalVariance = totalBudgeted - totalActual;
        decimal overallUtilization = totalBudgeted > 0
            ? Math.Round(((decimal)totalActual / totalBudgeted) * 100m, 2)
            : 0m;

        return new BudgetVarianceReport(
            Period: period,
            Currency: currency,
            TotalBudgeted: totalBudgeted,
            TotalActual: totalActual,
            TotalVariance: totalVariance,
            OverallUtilizationPercentage: overallUtilization,
            Categories: categoryReports.OrderByDescending(c => c.ActualAmount).ToList()
        );
    }

    public async Task<RecurringExpensesReport> GetRecurringExpensesAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR")
    {
        var expenses = (await _transactionRepository.GetAllAsync(
            startDate: startDate,
            endDate: endDate,
            type: "EXPENSE",
            currency: currency,
            limit: int.MaxValue)).ToList();

        long totalExpenses = expenses.Sum(t => t.Amount);

        var recurringList = expenses.Where(t => t.IsRecurring).ToList();
        var variableList = expenses.Where(t => !t.IsRecurring).ToList();

        long recAmount = recurringList.Sum(t => t.Amount);
        long varAmount = variableList.Sum(t => t.Amount);

        decimal recPercent = totalExpenses > 0 ? Math.Round(((decimal)recAmount / totalExpenses) * 100m, 2) : 0m;
        decimal varPercent = totalExpenses > 0 ? Math.Round(((decimal)varAmount / totalExpenses) * 100m, 2) : 0m;

        return new RecurringExpensesReport(
            Currency: currency,
            StartDate: startDate,
            EndDate: endDate,
            TotalExpenses: totalExpenses,
            Recurring: new RecurringExpenseBreakdown(recAmount, recPercent, recurringList.Count),
            Variable: new RecurringExpenseBreakdown(varAmount, varPercent, variableList.Count)
        );
    }

    public async Task<AccountBalancesReport> GetAccountBalancesAsync(string currency = "EUR")
    {
        var accounts = (await _accountRepository.GetAllAsync(currency: currency)).ToList();

        long netWorth = accounts.Sum(a => a.CurrentBalance);
        long liquidAssets = accounts
            .Where(a => a.AccountType.Equals("CHECKING", StringComparison.OrdinalIgnoreCase) ||
                        a.AccountType.Equals("SAVINGS", StringComparison.OrdinalIgnoreCase) ||
                        a.AccountType.Equals("CASH", StringComparison.OrdinalIgnoreCase))
            .Sum(a => a.CurrentBalance);

        var list = accounts.Select(a => new AccountBalanceItem(
            AccountId: a.Id,
            AccountName: a.Name,
            AccountType: a.AccountType,
            CurrentBalance: a.CurrentBalance
        )).ToList();

        return new AccountBalancesReport(
            Currency: currency,
            NetWorth: netWorth,
            LiquidAssets: liquidAssets,
            Accounts: list
        );
    }

    public string GenerateCsv<T>(IEnumerable<T> data, string reportTitle)
    {
        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties();

        sb.AppendLine($"# Report: {reportTitle}");
        sb.AppendLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(item)?.ToString()?.Replace("\"", "\"\"") ?? "";
                return $"\"{val}\"";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }
}

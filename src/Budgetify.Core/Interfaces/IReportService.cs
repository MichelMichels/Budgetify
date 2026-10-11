using Budgetify.Core.DTOs;

namespace Budgetify.Core.Interfaces;

public interface IReportService
{
    Task<ExpenseSummaryReport> GetExpenseSummaryAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR", string groupBy = "month");
    Task<CategoryBreakdownReport> GetCategoryBreakdownAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR", string flowType = "EXPENSE");
    Task<PartySpendReport> GetPartySpendAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR", string? partyType = null, int limit = 10);
    Task<BudgetVarianceReport> GetBudgetVarianceAsync(string period, string currency = "EUR");
    Task<RecurringExpensesReport> GetRecurringExpensesAsync(DateTimeOffset startDate, DateTimeOffset endDate, string currency = "EUR");
    Task<AccountBalancesReport> GetAccountBalancesAsync(string currency = "EUR");
    string GenerateCsv<T>(IEnumerable<T> data, string reportTitle);
}

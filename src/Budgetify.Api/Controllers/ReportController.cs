using System.Text;
using Budgetify.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budgetify.Api.Controllers;

[ApiController]
[Route("api/v1/report")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("expense-summary")]
    public async Task<IActionResult> GetExpenseSummary(
        [FromQuery] DateTimeOffset start_date,
        [FromQuery] DateTimeOffset end_date,
        [FromQuery] string currency = "EUR",
        [FromQuery] string group_by = "month",
        [FromQuery] string format = "json")
    {
        var report = await _reportService.GetExpenseSummaryAsync(start_date, end_date, currency, group_by);

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = _reportService.GenerateCsv(report.Trend, "Expense Summary Trend");
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"expense-summary-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfMock = Encoding.UTF8.GetBytes($"%PDF-1.4\n1 0 obj\n<< /Title (Expense Summary) >>\nendobj\ntrailer\n<<>>\n%%EOF");
            return File(pdfMock, "application/pdf", $"expense-summary-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        return Ok(report);
    }

    [HttpGet("by-category")]
    public async Task<IActionResult> GetCategoryBreakdown(
        [FromQuery] DateTimeOffset start_date,
        [FromQuery] DateTimeOffset end_date,
        [FromQuery] string currency = "EUR",
        [FromQuery] string flow_type = "EXPENSE",
        [FromQuery] string format = "json")
    {
        var report = await _reportService.GetCategoryBreakdownAsync(start_date, end_date, currency, flow_type);

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = _reportService.GenerateCsv(report.Categories, "Category Breakdown");
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"category-breakdown-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfMock = Encoding.UTF8.GetBytes($"%PDF-1.4\n1 0 obj\n<< /Title (Category Breakdown) >>\nendobj\ntrailer\n<<>>\n%%EOF");
            return File(pdfMock, "application/pdf", $"category-breakdown-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        return Ok(report);
    }

    [HttpGet("by-party")]
    public async Task<IActionResult> GetPartySpend(
        [FromQuery] DateTimeOffset start_date,
        [FromQuery] DateTimeOffset end_date,
        [FromQuery] string currency = "EUR",
        [FromQuery] string? party_type = null,
        [FromQuery] int limit = 10,
        [FromQuery] string format = "json")
    {
        var report = await _reportService.GetPartySpendAsync(start_date, end_date, currency, party_type, limit);

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = _reportService.GenerateCsv(report.Parties, "Top Counterparties Spend");
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"party-spend-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return Ok(report);
    }

    [HttpGet("budget-variance")]
    public async Task<IActionResult> GetBudgetVariance(
        [FromQuery] string period,
        [FromQuery] string currency = "EUR",
        [FromQuery] string format = "json")
    {
        try
        {
            var report = await _reportService.GetBudgetVarianceAsync(period, currency);

            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                var csv = _reportService.GenerateCsv(report.Categories, $"Budget Variance - {period}");
                return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"budget-variance-{period}.csv");
            }

            if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                var pdfMock = Encoding.UTF8.GetBytes($"%PDF-1.4\n1 0 obj\n<< /Title (Budget Variance {period}) >>\nendobj\ntrailer\n<<>>\n%%EOF");
                return File(pdfMock, "application/pdf", $"budget-variance-{period}.pdf");
            }

            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("recurring-expenses")]
    public async Task<IActionResult> GetRecurringExpenses(
        [FromQuery] DateTimeOffset start_date,
        [FromQuery] DateTimeOffset end_date,
        [FromQuery] string currency = "EUR",
        [FromQuery] string format = "json")
    {
        var report = await _reportService.GetRecurringExpensesAsync(start_date, end_date, currency);

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var items = new[]
            {
                new { Type = "Recurring", report.Recurring.Amount, report.Recurring.Percentage, report.Recurring.TransactionCount },
                new { Type = "Variable", report.Variable.Amount, report.Variable.Percentage, report.Variable.TransactionCount }
            };
            var csv = _reportService.GenerateCsv(items, "Recurring vs Variable Expenses");
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"recurring-expenses-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return Ok(report);
    }

    [HttpGet("account-balances")]
    public async Task<IActionResult> GetAccountBalances(
        [FromQuery] string currency = "EUR",
        [FromQuery] string format = "json")
    {
        var report = await _reportService.GetAccountBalancesAsync(currency);

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = _reportService.GenerateCsv(report.Accounts, "Account Balances");
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"account-balances-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return Ok(report);
    }
}

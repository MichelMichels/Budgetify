namespace Budgetify.Core.DTOs;

public record CreateBudgetRequest(
    Guid CategoryId,
    string? Period,
    long Amount,
    string? Currency,
    string? Notes
);

public record UpdateBudgetRequest(
    long? Amount,
    string? Notes
);

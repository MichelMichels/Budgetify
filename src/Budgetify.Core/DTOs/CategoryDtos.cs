namespace Budgetify.Core.DTOs;

public record CreateCategoryRequest(
    string Description,
    string? FlowType,
    bool? IsActive,
    string CategoryCode
);

public record UpdateCategoryRequest(
    string? Description,
    string? FlowType,
    bool? IsActive,
    string? CategoryCode
);

namespace Budgetify.Core.DTOs;

public record CreateAccountRequest(
    string Name,
    string AccountType,
    string? Currency,
    long? InitialBalance,
    string? Iban,
    bool? IsActive,
    string? Notes
);

public record UpdateAccountRequest(
    string? Name,
    string? AccountType,
    string? Iban,
    bool? IsActive,
    string? Notes
);

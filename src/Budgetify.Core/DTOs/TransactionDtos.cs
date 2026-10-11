namespace Budgetify.Core.DTOs;

public record CreateTransactionRequest(
    long Amount,
    string? Currency,
    string Type,
    Guid AccountId,
    Guid? DestinationAccountId,
    Guid? CategoryId,
    Guid? PartyId,
    string? PaymentMethod,
    bool? IsRecurring,
    DateTimeOffset TransactionDate,
    string Description,
    string? Notes
);

public record UpdateTransactionRequest(
    long? Amount,
    string? Currency,
    string? Type,
    Guid? AccountId,
    Guid? DestinationAccountId,
    Guid? CategoryId,
    Guid? PartyId,
    string? PaymentMethod,
    bool? IsRecurring,
    DateTimeOffset? TransactionDate,
    string? Description,
    string? Notes
);

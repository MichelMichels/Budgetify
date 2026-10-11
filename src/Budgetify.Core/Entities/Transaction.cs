namespace Budgetify.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long Amount { get; set; } // in cents, non-zero, negative for refunds
    public string Currency { get; set; } = "EUR";
    public string Type { get; set; } = "EXPENSE"; // EXPENSE, INCOME, TRANSFER
    public Guid AccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? PartyId { get; set; }
    public string? PaymentMethod { get; set; } // CREDIT_CARD, DEBIT_CARD, BANK_TRANSFER, CASH, OTHER
    public bool IsRecurring { get; set; } = false;
    public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

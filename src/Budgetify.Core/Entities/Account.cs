namespace Budgetify.Core.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "CHECKING"; // CHECKING, SAVINGS, CREDIT_CARD, CASH, INVESTMENT
    public string Currency { get; set; } = "EUR";
    public long CurrentBalance { get; set; } = 0; // In cents
    public string? Iban { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

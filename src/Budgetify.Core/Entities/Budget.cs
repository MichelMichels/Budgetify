namespace Budgetify.Core.Entities;

public class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public string? Period { get; set; } // "YYYY-MM" or null for recurring baseline
    public bool IsRecurring => string.IsNullOrEmpty(Period);
    public long Amount { get; set; } // In cents, > 0
    public string Currency { get; set; } = "EUR";
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

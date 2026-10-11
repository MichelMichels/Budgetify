namespace Budgetify.Core.Entities;

public class Party
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty; // MERCHANT, EMPLOYER, INDIVIDUAL, ORGANIZATION
    public bool IsActive { get; set; } = true;
    public string PartyCode { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

namespace Budgetify.Core.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public string FlowType { get; set; } = "EXPENSE"; // EXPENSE or INCOME
    public bool IsActive { get; set; } = true;
    public string CategoryCode { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

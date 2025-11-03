namespace MyApp.Core.Entities;

/// <summary>
/// Represents a financial product
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty; // Loan, Credit Card, Savings Account, etc.
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; } = string.Empty;
}



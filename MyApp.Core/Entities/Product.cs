namespace MyApp.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty; // Loan, CreditCard, SavingsAccount, etc.
    public decimal InterestRate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}


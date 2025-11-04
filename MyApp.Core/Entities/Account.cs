namespace MyApp.Core.Entities;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string AccountType { get; set; } = string.Empty; // Savings, Checking, etc.
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}


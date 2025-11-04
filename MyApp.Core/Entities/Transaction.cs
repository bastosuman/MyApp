namespace MyApp.Core.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal, Transfer, etc.
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } = "Completed"; // Completed, Pending, Failed
    
    // Navigation property
    public virtual Account Account { get; set; } = null!;
}


namespace MyApp.Core.Entities;

/// <summary>
/// Represents a financial transaction
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal, Payment, etc.
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed
    
    // Navigation property
    public virtual Account? Account { get; set; }
}



namespace MyApp.Core.Entities;

/// <summary>
/// Represents a financial application (loan, credit, etc.)
/// </summary>
public class Application
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string ApplicationType { get; set; } = string.Empty; // Loan, Credit Card, etc.
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected, Completed
    public DateTime ApplicationDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    
    // Navigation property
    public virtual Account? Account { get; set; }
}



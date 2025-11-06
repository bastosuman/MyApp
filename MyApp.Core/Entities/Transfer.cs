namespace MyApp.Core.Entities;

public class Transfer
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public int? DestinationAccountId { get; set; } // Nullable for external transfers
    public string? DestinationAccountNumber { get; set; } // For external transfers
    public string TransferType { get; set; } = string.Empty; // "Internal", "External"
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // "Pending", "Processing", "Completed", "Failed", "Cancelled"
    public DateTime TransferDate { get; set; }
    public DateTime? ScheduledDate { get; set; } // For scheduled transfers
    public string? RecurrencePattern { get; set; } // JSON: { "Type": "Monthly", "Day": 15 }
    public int? SourceTransactionId { get; set; }
    public int? DestinationTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // Navigation properties
    public virtual Account SourceAccount { get; set; } = null!;
    public virtual Account? DestinationAccount { get; set; }
    public virtual Transaction? SourceTransaction { get; set; }
    public virtual Transaction? DestinationTransaction { get; set; }
}



namespace MyApp.Core.Entities;

public class ScheduledTransfer
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public int? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string TransferType { get; set; } = string.Empty; // "Internal", "External"
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string RecurrenceType { get; set; } = "OneTime"; // "OneTime", "Daily", "Weekly", "Monthly", "Quarterly", "Annually"
    public int? RecurrenceDay { get; set; } // Day of month or day of week
    public string Status { get; set; } = "Active"; // "Active", "Paused", "Completed", "Cancelled"
    public DateTime? NextExecutionDate { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public int ExecutionCount { get; set; } = 0;
    public DateTime CreatedDate { get; set; }
    
    // Navigation properties
    public virtual Account SourceAccount { get; set; } = null!;
    public virtual Account? DestinationAccount { get; set; }
}



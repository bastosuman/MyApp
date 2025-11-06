using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.DTOs;

public class ScheduledTransferDto
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public int? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string TransferType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string RecurrenceType { get; set; } = string.Empty;
    public int? RecurrenceDay { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? NextExecutionDate { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateScheduledTransferDto
{
    [Required]
    public int SourceAccountId { get; set; }
    
    public int? DestinationAccountId { get; set; }
    
    [StringLength(50)]
    public string? DestinationAccountNumber { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public DateTime ScheduledDate { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RecurrenceType { get; set; } = "OneTime"; // "OneTime", "Daily", "Weekly", "Monthly", "Quarterly", "Annually"
    
    public int? RecurrenceDay { get; set; } // For weekly/monthly
}

public class UpdateScheduledTransferDto
{
    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
    
    [StringLength(50)]
    public string? RecurrenceType { get; set; }
    
    public int? RecurrenceDay { get; set; }
}



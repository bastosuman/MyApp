using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.DTOs;

public class TransferDto
{
    public int Id { get; set; }
    public int SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public int? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string TransferType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? FailureReason { get; set; }
}

public class CreateInternalTransferDto
{
    [Required]
    public int SourceAccountId { get; set; }
    
    [Required]
    public int DestinationAccountId { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? ScheduledDate { get; set; }
}

public class CreateExternalTransferDto
{
    [Required]
    public int SourceAccountId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DestinationAccountNumber { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? ScheduledDate { get; set; }
}

public class AccountLimitsDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal DailyTransferLimit { get; set; }
    public decimal MonthlyTransferLimit { get; set; }
    public decimal PerTransactionMax { get; set; }
    public decimal PerTransactionMin { get; set; }
    public decimal DailyTransferUsed { get; set; }
    public decimal MonthlyTransferUsed { get; set; }
    public DateTime? LastDailyReset { get; set; }
    public DateTime? LastMonthlyReset { get; set; }
}

public class UpdateAccountLimitsDto
{
    [Range(0.01, double.MaxValue)]
    public decimal? DailyTransferLimit { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? MonthlyTransferLimit { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? PerTransactionMax { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? PerTransactionMin { get; set; }
}



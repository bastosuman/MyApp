using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Services;

/// <summary>
/// Parameters for transfer execution to reduce method parameter count
/// </summary>
internal class TransferExecutionParameters
{
    public FinancialDbContext Context { get; set; } = null!;
    public Account SourceAccount { get; set; } = null!;
    public Account DestinationAccount { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public TransferValidationResult Validation { get; set; } = null!;
}


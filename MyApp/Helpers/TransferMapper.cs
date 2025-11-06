using MyApp.Core.DTOs;
using MyApp.Core.Entities;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in transfer mapping logic
/// </summary>
public static class TransferMapper
{
    /// <summary>
    /// Maps Transfer entity to TransferDto
    /// </summary>
    public static TransferDto MapToDto(Transfer transfer)
    {
        return new TransferDto
        {
            Id = transfer.Id,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountNumber = transfer.SourceAccount?.AccountNumber ?? string.Empty,
            DestinationAccountId = transfer.DestinationAccountId,
            DestinationAccountNumber = transfer.DestinationAccount?.AccountNumber ?? transfer.DestinationAccountNumber,
            TransferType = transfer.TransferType,
            Amount = transfer.Amount,
            Description = transfer.Description,
            Status = transfer.Status,
            TransferDate = transfer.TransferDate,
            ScheduledDate = transfer.ScheduledDate,
            CompletedDate = transfer.CompletedDate,
            FailureReason = transfer.FailureReason
        };
    }

    /// <summary>
    /// Maps ScheduledTransfer entity to ScheduledTransferDto
    /// </summary>
    public static ScheduledTransferDto MapToDto(ScheduledTransfer scheduledTransfer)
    {
        return new ScheduledTransferDto
        {
            Id = scheduledTransfer.Id,
            SourceAccountId = scheduledTransfer.SourceAccountId,
            SourceAccountNumber = scheduledTransfer.SourceAccount?.AccountNumber ?? string.Empty,
            DestinationAccountId = scheduledTransfer.DestinationAccountId,
            DestinationAccountNumber = scheduledTransfer.DestinationAccount?.AccountNumber ?? scheduledTransfer.DestinationAccountNumber,
            TransferType = scheduledTransfer.TransferType,
            Amount = scheduledTransfer.Amount,
            Description = scheduledTransfer.Description,
            ScheduledDate = scheduledTransfer.ScheduledDate,
            RecurrenceType = scheduledTransfer.RecurrenceType,
            RecurrenceDay = scheduledTransfer.RecurrenceDay,
            Status = scheduledTransfer.Status,
            NextExecutionDate = scheduledTransfer.NextExecutionDate,
            LastExecutionDate = scheduledTransfer.LastExecutionDate,
            ExecutionCount = scheduledTransfer.ExecutionCount,
            CreatedDate = scheduledTransfer.CreatedDate
        };
    }
}


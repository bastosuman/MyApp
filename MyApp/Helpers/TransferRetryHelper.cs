using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Services;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in transfer retry logic
/// </summary>
public static class TransferRetryHelper
{
    /// <summary>
    /// Creates the appropriate DTO for retrying a transfer based on its type
    /// </summary>
    public static async Task<TransferExecutionResult> RetryTransferAsync(
        Transfer transfer,
        TransferService transferService)
    {
        if (transfer.TransferType == "Internal" && transfer.DestinationAccountId.HasValue)
        {
            var dto = new CreateInternalTransferDto
            {
                SourceAccountId = transfer.SourceAccountId,
                DestinationAccountId = transfer.DestinationAccountId.Value,
                Amount = transfer.Amount,
                Description = transfer.Description
            };
            return await transferService.ExecuteInternalTransferAsync(dto);
        }
        else if (transfer.TransferType == "External" && !string.IsNullOrWhiteSpace(transfer.DestinationAccountNumber))
        {
            var dto = new CreateExternalTransferDto
            {
                SourceAccountId = transfer.SourceAccountId,
                DestinationAccountNumber = transfer.DestinationAccountNumber,
                Amount = transfer.Amount,
                Description = transfer.Description
            };
            return await transferService.ExecuteExternalTransferAsync(dto);
        }

        return new TransferExecutionResult
        {
            Success = false,
            ErrorMessage = "Invalid transfer type for retry"
        };
    }
}


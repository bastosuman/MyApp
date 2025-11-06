using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Services;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in transfer creation logic
/// </summary>
public static class TransferCreationHelper
{
    /// <summary>
    /// Handles the common pattern of executing a transfer and returning the result
    /// </summary>
    public static async Task<ActionResult<ApiResponse<TransferDto>>> HandleTransferCreationAsync(
        FinancialDbContext context,
        TransferExecutionResult result,
        string actionName)
    {
        if (!result.Success)
        {
            return ControllerErrorHandler.BadRequestResponse<TransferDto>(result.ErrorMessage);
        }

        var transfer = await TransferQueryHelper.GetTransferWithIncludes(context)
            .FirstOrDefaultAsync(t => t.Id == result.TransferId);

        if (transfer == null)
        {
            return ControllerErrorHandler.EntityCreatedButNotFoundResponse<TransferDto>("Transfer");
        }

        var transferDto = TransferMapper.MapToDto(transfer);
        return new CreatedAtActionResult(
            actionName,
            "Transfers",
            new { id = transfer.Id },
            ApiResponse<TransferDto>.SuccessResponse(transferDto, result.Message));
    }
}


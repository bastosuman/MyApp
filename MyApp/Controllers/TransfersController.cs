using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Helpers;
using MyApp.Services;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly TransferService _transferService;
    private readonly ILogger<TransfersController> _logger;

    public TransfersController(
        FinancialDbContext context,
        TransferService transferService,
        ILogger<TransfersController> logger)
    {
        _context = context;
        _transferService = transferService;
        _logger = logger;
    }

    /// <summary>
    /// Create an internal transfer (between own accounts)
    /// </summary>
    [HttpPost("internal")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> CreateInternalTransfer(CreateInternalTransferDto dto)
    {
        return await ControllerActionHelper.ExecuteWithValidationAndErrorHandling<TransferDto>(
            this,
            _logger,
            "creating internal transfer",
            "An error occurred while creating the transfer",
            async () =>
            {
                var result = await _transferService.ExecuteInternalTransferAsync(dto);
                return await TransferCreationHelper.HandleTransferCreationAsync(_context, result, nameof(GetTransfer));
            });
    }

    /// <summary>
    /// Create an external transfer (to another account by account number)
    /// </summary>
    [HttpPost("external")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> CreateExternalTransfer(CreateExternalTransferDto dto)
    {
        return await ControllerActionHelper.ExecuteWithValidationAndErrorHandling<TransferDto>(
            this,
            _logger,
            "creating external transfer",
            "An error occurred while creating the transfer",
            async () =>
            {
                var result = await _transferService.ExecuteExternalTransferAsync(dto);
                return await TransferCreationHelper.HandleTransferCreationAsync(_context, result, nameof(GetTransfer));
            });
    }

    /// <summary>
    /// Get all transfers with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransferDto>>>> GetTransfers(
        [FromQuery] int? accountId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? transferType = null)
    {
        try
        {
            var query = TransferQueryHelper.GetTransferWithIncludes(_context).AsQueryable();

            if (accountId.HasValue)
            {
                query = query.Where(t => t.SourceAccountId == accountId.Value || 
                                        (t.DestinationAccountId.HasValue && t.DestinationAccountId == accountId.Value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(transferType))
            {
                query = query.Where(t => t.TransferType == transferType);
            }

            var transfers = await query
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();

            var transferDtos = transfers.Select(TransferMapper.MapToDto).ToList();

            return Ok(ApiResponse<IEnumerable<TransferDto>>.SuccessResponse(transferDtos, "Transfers retrieved successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<IEnumerable<TransferDto>>(
                ex, _logger, "retrieving transfers", "An error occurred while retrieving transfers");
        }
    }

    /// <summary>
    /// Get transfer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> GetTransfer(int id)
    {
        try
        {
            var transfer = await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<TransferDto>($"Transfer with ID {id} not found");
            }

            var transferDto = TransferMapper.MapToDto(transfer);
            return Ok(ApiResponse<TransferDto>.SuccessResponse(transferDto, "Transfer retrieved successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<TransferDto>(
                ex, _logger, "retrieving transfer", "An error occurred while retrieving the transfer");
        }
    }

    /// <summary>
    /// Get transfers for a specific account
    /// </summary>
    [HttpGet("accounts/{accountId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransferDto>>>> GetAccountTransfers(int accountId)
    {
        try
        {
            var transfers = await TransferQueryHelper.GetTransferWithIncludes(_context)
                .Where(t => t.SourceAccountId == accountId || 
                           (t.DestinationAccountId.HasValue && t.DestinationAccountId == accountId))
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();

            var transferDtos = transfers.Select(TransferMapper.MapToDto).ToList();

            return Ok(ApiResponse<IEnumerable<TransferDto>>.SuccessResponse(transferDtos, "Account transfers retrieved successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<IEnumerable<TransferDto>>(
                ex, _logger, "retrieving account transfers", "An error occurred while retrieving account transfers");
        }
    }

    /// <summary>
    /// Cancel a pending transfer
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> CancelTransfer(int id)
    {
        try
        {
            var cancelled = await _transferService.CancelTransferAsync(id);

            if (!cancelled)
            {
                return ControllerErrorHandler.BadRequestResponse<object>("Transfer cannot be cancelled. It may not exist or is not in a cancellable state.");
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Transfer cancelled successfully" }));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<object>(
                ex, _logger, "cancelling transfer", "An error occurred while cancelling the transfer");
        }
    }

    /// <summary>
    /// Retry a failed transfer
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> RetryTransfer(int id)
    {
        try
        {
            var transfer = await TransferQueryHelper.GetTransferWithIncludes(_context)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<TransferDto>($"Transfer with ID {id} not found");
            }

            if (transfer.Status != "Failed")
            {
                return ControllerErrorHandler.BadRequestResponse<TransferDto>("Only failed transfers can be retried");
            }

            // Retry logic - execute the transfer again
            var result = await TransferRetryHelper.RetryTransferAsync(transfer, _transferService);
            
            if (!result.Success && result.ErrorMessage == "Invalid transfer type for retry")
            {
                return ControllerErrorHandler.BadRequestResponse<TransferDto>(result.ErrorMessage);
            }

            if (!result.Success)
            {
                return ControllerErrorHandler.BadRequestResponse<TransferDto>(result.ErrorMessage);
            }

            var retriedTransfer = await TransferQueryHelper.GetTransferWithIncludes(_context)
                .FirstOrDefaultAsync(t => t.Id == result.TransferId);

            if (retriedTransfer == null)
            {
                return ControllerErrorHandler.EntityCreatedButNotFoundResponse<TransferDto>("Transfer");
            }

            var transferDto = TransferMapper.MapToDto(retriedTransfer);
            return Ok(ApiResponse<TransferDto>.SuccessResponse(transferDto, "Transfer retried successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<TransferDto>(
                ex, _logger, "retrying transfer", "An error occurred while retrying the transfer");
        }
    }

}



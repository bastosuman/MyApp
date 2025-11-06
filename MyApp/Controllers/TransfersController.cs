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
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<TransferDto>(this);
            if (validationError != null) return validationError;

            var result = await _transferService.ExecuteInternalTransferAsync(dto);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<TransferDto>.ErrorResponse(result.ErrorMessage));
            }

            var transfer = await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .FirstOrDefaultAsync(t => t.Id == result.TransferId);

            if (transfer == null)
            {
                return StatusCode(500, ApiResponse<TransferDto>.ErrorResponse("Transfer created but could not be retrieved"));
            }

            var transferDto = MapToDto(transfer);
            return CreatedAtAction(
                nameof(GetTransfer),
                new { id = transfer.Id },
                ApiResponse<TransferDto>.SuccessResponse(transferDto, result.Message));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<TransferDto>(
                ex, _logger, "creating internal transfer", "An error occurred while creating the transfer");
        }
    }

    /// <summary>
    /// Create an external transfer (to another account by account number)
    /// </summary>
    [HttpPost("external")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> CreateExternalTransfer(CreateExternalTransferDto dto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<TransferDto>(this);
            if (validationError != null) return validationError;

            var result = await _transferService.ExecuteExternalTransferAsync(dto);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<TransferDto>.ErrorResponse(result.ErrorMessage));
            }

            var transfer = await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .FirstOrDefaultAsync(t => t.Id == result.TransferId);

            if (transfer == null)
            {
                return StatusCode(500, ApiResponse<TransferDto>.ErrorResponse("Transfer created but could not be retrieved"));
            }

            var transferDto = MapToDto(transfer);
            return CreatedAtAction(
                nameof(GetTransfer),
                new { id = transfer.Id },
                ApiResponse<TransferDto>.SuccessResponse(transferDto, result.Message));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<TransferDto>(
                ex, _logger, "creating external transfer", "An error occurred while creating the transfer");
        }
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
            var query = _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .AsQueryable();

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

            var transferDtos = transfers.Select(MapToDto).ToList();

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
                return NotFound(ApiResponse<TransferDto>.ErrorResponse($"Transfer with ID {id} not found"));
            }

            var transferDto = MapToDto(transfer);
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
            var transfers = await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .Where(t => t.SourceAccountId == accountId || 
                           (t.DestinationAccountId.HasValue && t.DestinationAccountId == accountId))
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();

            var transferDtos = transfers.Select(MapToDto).ToList();

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
                return BadRequest(ApiResponse<object>.ErrorResponse("Transfer cannot be cancelled. It may not exist or is not in a cancellable state."));
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
            var transfer = await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
            {
                return NotFound(ApiResponse<TransferDto>.ErrorResponse($"Transfer with ID {id} not found"));
            }

            if (transfer.Status != "Failed")
            {
                return BadRequest(ApiResponse<TransferDto>.ErrorResponse("Only failed transfers can be retried"));
            }

            // Retry logic - execute the transfer again
            TransferExecutionResult result;
            if (transfer.TransferType == "Internal" && transfer.DestinationAccountId.HasValue)
            {
                var dto = new CreateInternalTransferDto
                {
                    SourceAccountId = transfer.SourceAccountId,
                    DestinationAccountId = transfer.DestinationAccountId.Value,
                    Amount = transfer.Amount,
                    Description = transfer.Description
                };
                result = await _transferService.ExecuteInternalTransferAsync(dto);
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
                result = await _transferService.ExecuteExternalTransferAsync(dto);
            }
            else
            {
                return BadRequest(ApiResponse<TransferDto>.ErrorResponse("Invalid transfer type for retry"));
            }

            if (!result.Success)
            {
                return BadRequest(ApiResponse<TransferDto>.ErrorResponse(result.ErrorMessage));
            }

            var retriedTransfer = await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .FirstOrDefaultAsync(t => t.Id == result.TransferId);

            if (retriedTransfer == null)
            {
                return StatusCode(500, ApiResponse<TransferDto>.ErrorResponse("Transfer retried but could not be retrieved"));
            }

            var transferDto = MapToDto(retriedTransfer);
            return Ok(ApiResponse<TransferDto>.SuccessResponse(transferDto, "Transfer retried successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<TransferDto>(
                ex, _logger, "retrying transfer", "An error occurred while retrying the transfer");
        }
    }

    private TransferDto MapToDto(Core.Entities.Transfer transfer)
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
}



using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Helpers;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduledTransfersController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<ScheduledTransfersController> _logger;

    public ScheduledTransfersController(FinancialDbContext context, ILogger<ScheduledTransfersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a scheduled or recurring transfer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ScheduledTransferDto>>> CreateScheduledTransfer(CreateScheduledTransferDto dto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<ScheduledTransferDto>(this);
            if (validationError != null) return validationError;

            // Validate source account
            var sourceAccountError = await AccountValidator.ValidateSourceAccountAsync<ScheduledTransferDto>(_context, dto.SourceAccountId);
            if (sourceAccountError != null) return sourceAccountError;
            var sourceAccount = await _context.Accounts.FindAsync(dto.SourceAccountId);

            // Validate destination account
            Account? destinationAccount = null;
            if (dto.DestinationAccountId.HasValue)
            {
                var destError = await AccountValidator.ValidateDestinationAccountByIdAsync<ScheduledTransferDto>(_context, dto.DestinationAccountId.Value);
                if (destError != null) return destError;
                destinationAccount = await _context.Accounts.FindAsync(dto.DestinationAccountId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(dto.DestinationAccountNumber))
            {
                var (destError, destAccount) = await AccountValidator.ValidateDestinationAccountByNumberAsync<ScheduledTransferDto>(_context, dto.DestinationAccountNumber);
                if (destError != null) return destError;
                destinationAccount = destAccount;
            }
            else
            {
                return ControllerErrorHandler.BadRequestResponse<ScheduledTransferDto>("Either DestinationAccountId or DestinationAccountNumber must be provided");
            }

            // Determine transfer type
            var transferType = dto.DestinationAccountId.HasValue ? "Internal" : "External";

            // Calculate next execution date
            var nextExecutionDate = RecurrenceCalculator.CalculateNextExecutionDate(dto.ScheduledDate, dto.RecurrenceType, dto.RecurrenceDay);

            var scheduledTransfer = new ScheduledTransfer
            {
                SourceAccountId = dto.SourceAccountId,
                DestinationAccountId = destinationAccount.Id,
                DestinationAccountNumber = dto.DestinationAccountNumber,
                TransferType = transferType,
                Amount = dto.Amount,
                Description = dto.Description,
                ScheduledDate = dto.ScheduledDate,
                RecurrenceType = dto.RecurrenceType,
                RecurrenceDay = dto.RecurrenceDay,
                Status = "Active",
                NextExecutionDate = nextExecutionDate,
                CreatedDate = DateTime.UtcNow
            };

            _context.ScheduledTransfers.Add(scheduledTransfer);
            await _context.SaveChangesAsync();

            var scheduledTransferDto = TransferMapper.MapToDto(scheduledTransfer);
            return CreatedAtAction(
                nameof(GetScheduledTransfer),
                new { id = scheduledTransfer.Id },
                ApiResponse<ScheduledTransferDto>.SuccessResponse(scheduledTransferDto, "Scheduled transfer created successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<ScheduledTransferDto>(
                ex, _logger, "creating scheduled transfer", "An error occurred while creating the scheduled transfer");
        }
    }

    /// <summary>
    /// Get all scheduled transfers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ScheduledTransferDto>>>> GetScheduledTransfers(
        [FromQuery] int? accountId = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var query = TransferQueryHelper.GetScheduledTransferWithIncludes(_context).AsQueryable();

            if (accountId.HasValue)
            {
                query = query.Where(s => s.SourceAccountId == accountId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }

            var scheduledTransfers = await query
                .OrderBy(s => s.NextExecutionDate)
                .ToListAsync();

            var dtos = scheduledTransfers.Select(TransferMapper.MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<ScheduledTransferDto>>.SuccessResponse(dtos, "Scheduled transfers retrieved successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<IEnumerable<ScheduledTransferDto>>(
                ex, _logger, "retrieving scheduled transfers", "An error occurred while retrieving scheduled transfers");
        }
    }

    /// <summary>
    /// Get scheduled transfer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ScheduledTransferDto>>> GetScheduledTransfer(int id)
    {
        try
        {
            var scheduledTransfer = await TransferQueryHelper.GetScheduledTransferWithIncludes(_context)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scheduledTransfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<ScheduledTransferDto>($"Scheduled transfer with ID {id} not found");
            }

            var dto = TransferMapper.MapToDto(scheduledTransfer);
            return Ok(ApiResponse<ScheduledTransferDto>.SuccessResponse(dto, "Scheduled transfer retrieved successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<ScheduledTransferDto>(
                ex, _logger, "retrieving scheduled transfer", "An error occurred while retrieving the scheduled transfer");
        }
    }

    /// <summary>
    /// Update a scheduled transfer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ScheduledTransferDto>>> UpdateScheduledTransfer(int id, UpdateScheduledTransferDto dto)
    {
        try
        {
            var scheduledTransfer = await _context.ScheduledTransfers
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scheduledTransfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<ScheduledTransferDto>($"Scheduled transfer with ID {id} not found");
            }

            if (scheduledTransfer.Status != "Active" && scheduledTransfer.Status != "Paused")
            {
                return ControllerErrorHandler.BadRequestResponse<ScheduledTransferDto>("Only active or paused scheduled transfers can be updated");
            }

            if (dto.Amount.HasValue)
            {
                scheduledTransfer.Amount = dto.Amount.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                scheduledTransfer.Description = dto.Description;
            }

            if (dto.ScheduledDate.HasValue)
            {
                scheduledTransfer.ScheduledDate = dto.ScheduledDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.RecurrenceType))
            {
                scheduledTransfer.RecurrenceType = dto.RecurrenceType;
            }

            if (dto.RecurrenceDay.HasValue)
            {
                scheduledTransfer.RecurrenceDay = dto.RecurrenceDay.Value;
            }

            // Recalculate next execution date
            scheduledTransfer.NextExecutionDate = RecurrenceCalculator.CalculateNextExecutionDate(
                scheduledTransfer.ScheduledDate,
                scheduledTransfer.RecurrenceType,
                scheduledTransfer.RecurrenceDay);

            await _context.SaveChangesAsync();

            var updatedTransfer = await TransferQueryHelper.GetScheduledTransferWithIncludes(_context)
                .FirstOrDefaultAsync(s => s.Id == id);

            var transferDto = TransferMapper.MapToDto(updatedTransfer!);
            return Ok(ApiResponse<ScheduledTransferDto>.SuccessResponse(transferDto, "Scheduled transfer updated successfully"));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<ScheduledTransferDto>(
                ex, _logger, "updating scheduled transfer", "An error occurred while updating the scheduled transfer");
        }
    }

    /// <summary>
    /// Cancel a scheduled transfer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> CancelScheduledTransfer(int id)
    {
        try
        {
            var scheduledTransfer = await _context.ScheduledTransfers.FindAsync(id);

            if (scheduledTransfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<object>($"Scheduled transfer with ID {id} not found");
            }

            scheduledTransfer.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Scheduled transfer cancelled successfully" }));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<object>(
                ex, _logger, "cancelling scheduled transfer", "An error occurred while cancelling the scheduled transfer");
        }
    }

    /// <summary>
    /// Pause a recurring transfer
    /// </summary>
    [HttpPut("{id}/pause")]
    public async Task<ActionResult<ApiResponse<object>>> PauseScheduledTransfer(int id)
    {
        try
        {
            var scheduledTransfer = await _context.ScheduledTransfers.FindAsync(id);

            if (scheduledTransfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<object>($"Scheduled transfer with ID {id} not found");
            }

            if (scheduledTransfer.Status != "Active")
            {
                return ControllerErrorHandler.BadRequestResponse<object>("Only active scheduled transfers can be paused");
            }

            scheduledTransfer.Status = "Paused";
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Scheduled transfer paused successfully" }));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<object>(
                ex, _logger, "pausing scheduled transfer", "An error occurred while pausing the scheduled transfer");
        }
    }

    /// <summary>
    /// Resume a paused recurring transfer
    /// </summary>
    [HttpPut("{id}/resume")]
    public async Task<ActionResult<ApiResponse<object>>> ResumeScheduledTransfer(int id)
    {
        try
        {
            var scheduledTransfer = await _context.ScheduledTransfers.FindAsync(id);

            if (scheduledTransfer == null)
            {
                return ControllerErrorHandler.NotFoundResponse<object>($"Scheduled transfer with ID {id} not found");
            }

            if (scheduledTransfer.Status != "Paused")
            {
                return ControllerErrorHandler.BadRequestResponse<object>("Only paused scheduled transfers can be resumed");
            }

            scheduledTransfer.Status = "Active";
            scheduledTransfer.NextExecutionDate = RecurrenceCalculator.CalculateNextExecutionDate(
                scheduledTransfer.ScheduledDate,
                scheduledTransfer.RecurrenceType,
                scheduledTransfer.RecurrenceDay);

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Scheduled transfer resumed successfully" }));
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<object>(
                ex, _logger, "resuming scheduled transfer", "An error occurred while resuming the scheduled transfer");
        }
    }


}



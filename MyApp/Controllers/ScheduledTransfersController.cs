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

            // Validate accounts
            var sourceAccount = await _context.Accounts.FindAsync(dto.SourceAccountId);
            if (sourceAccount == null || !sourceAccount.IsActive)
            {
                return BadRequest(ApiResponse<ScheduledTransferDto>.ErrorResponse("Source account not found or inactive"));
            }

            Account? destinationAccount = null;
            if (dto.DestinationAccountId.HasValue)
            {
                destinationAccount = await _context.Accounts.FindAsync(dto.DestinationAccountId.Value);
                if (destinationAccount == null || !destinationAccount.IsActive)
                {
                    return BadRequest(ApiResponse<ScheduledTransferDto>.ErrorResponse("Destination account not found or inactive"));
                }
            }
            else if (!string.IsNullOrWhiteSpace(dto.DestinationAccountNumber))
            {
                destinationAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == dto.DestinationAccountNumber);
                if (destinationAccount == null || !destinationAccount.IsActive)
                {
                    return BadRequest(ApiResponse<ScheduledTransferDto>.ErrorResponse("Destination account not found or inactive"));
                }
            }
            else
            {
                return BadRequest(ApiResponse<ScheduledTransferDto>.ErrorResponse("Either DestinationAccountId or DestinationAccountNumber must be provided"));
            }

            // Determine transfer type
            var transferType = dto.DestinationAccountId.HasValue ? "Internal" : "External";

            // Calculate next execution date
            var nextExecutionDate = CalculateNextExecutionDate(dto.ScheduledDate, dto.RecurrenceType, dto.RecurrenceDay);

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

            var scheduledTransferDto = MapToDto(scheduledTransfer);
            return CreatedAtAction(
                nameof(GetScheduledTransfer),
                new { id = scheduledTransfer.Id },
                ApiResponse<ScheduledTransferDto>.SuccessResponse(scheduledTransferDto, "Scheduled transfer created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating scheduled transfer");
            return StatusCode(500, ApiResponse<ScheduledTransferDto>.ErrorResponse("An error occurred while creating the scheduled transfer"));
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
            var query = _context.ScheduledTransfers
                .Include(s => s.SourceAccount)
                .Include(s => s.DestinationAccount)
                .AsQueryable();

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

            var dtos = scheduledTransfers.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<ScheduledTransferDto>>.SuccessResponse(dtos, "Scheduled transfers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scheduled transfers");
            return StatusCode(500, ApiResponse<IEnumerable<ScheduledTransferDto>>.ErrorResponse("An error occurred while retrieving scheduled transfers"));
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
            var scheduledTransfer = await _context.ScheduledTransfers
                .Include(s => s.SourceAccount)
                .Include(s => s.DestinationAccount)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scheduledTransfer == null)
            {
                return NotFound(ApiResponse<ScheduledTransferDto>.ErrorResponse($"Scheduled transfer with ID {id} not found"));
            }

            var dto = MapToDto(scheduledTransfer);
            return Ok(ApiResponse<ScheduledTransferDto>.SuccessResponse(dto, "Scheduled transfer retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scheduled transfer");
            return StatusCode(500, ApiResponse<ScheduledTransferDto>.ErrorResponse("An error occurred while retrieving the scheduled transfer"));
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
                return NotFound(ApiResponse<ScheduledTransferDto>.ErrorResponse($"Scheduled transfer with ID {id} not found"));
            }

            if (scheduledTransfer.Status != "Active" && scheduledTransfer.Status != "Paused")
            {
                return BadRequest(ApiResponse<ScheduledTransferDto>.ErrorResponse("Only active or paused scheduled transfers can be updated"));
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
            scheduledTransfer.NextExecutionDate = CalculateNextExecutionDate(
                scheduledTransfer.ScheduledDate,
                scheduledTransfer.RecurrenceType,
                scheduledTransfer.RecurrenceDay);

            await _context.SaveChangesAsync();

            var updatedTransfer = await _context.ScheduledTransfers
                .Include(s => s.SourceAccount)
                .Include(s => s.DestinationAccount)
                .FirstOrDefaultAsync(s => s.Id == id);

            var transferDto = MapToDto(updatedTransfer!);
            return Ok(ApiResponse<ScheduledTransferDto>.SuccessResponse(transferDto, "Scheduled transfer updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scheduled transfer");
            return StatusCode(500, ApiResponse<ScheduledTransferDto>.ErrorResponse("An error occurred while updating the scheduled transfer"));
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
                return NotFound(ApiResponse<object>.ErrorResponse($"Scheduled transfer with ID {id} not found"));
            }

            scheduledTransfer.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Scheduled transfer cancelled successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling scheduled transfer");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while cancelling the scheduled transfer"));
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
                return NotFound(ApiResponse<object>.ErrorResponse($"Scheduled transfer with ID {id} not found"));
            }

            if (scheduledTransfer.Status != "Active")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Only active scheduled transfers can be paused"));
            }

            scheduledTransfer.Status = "Paused";
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Scheduled transfer paused successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing scheduled transfer");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while pausing the scheduled transfer"));
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
                return NotFound(ApiResponse<object>.ErrorResponse($"Scheduled transfer with ID {id} not found"));
            }

            if (scheduledTransfer.Status != "Paused")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Only paused scheduled transfers can be resumed"));
            }

            scheduledTransfer.Status = "Active";
            scheduledTransfer.NextExecutionDate = CalculateNextExecutionDate(
                scheduledTransfer.ScheduledDate,
                scheduledTransfer.RecurrenceType,
                scheduledTransfer.RecurrenceDay);

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Scheduled transfer resumed successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming scheduled transfer");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while resuming the scheduled transfer"));
        }
    }

    private ScheduledTransferDto MapToDto(ScheduledTransfer scheduledTransfer)
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

    private DateTime? CalculateNextExecutionDate(DateTime scheduledDate, string recurrenceType, int? recurrenceDay)
    {
        if (recurrenceType == "OneTime")
        {
            return scheduledDate;
        }

        var now = DateTime.UtcNow;
        var nextDate = scheduledDate;

        switch (recurrenceType)
        {
            case "Daily":
                nextDate = now.AddDays(1);
                break;

            case "Weekly":
                var daysUntilNext = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                if (daysUntilNext == 0) daysUntilNext = 7;
                nextDate = now.AddDays(daysUntilNext);
                break;

            case "Monthly":
                if (recurrenceDay.HasValue)
                {
                    var day = Math.Min(recurrenceDay.Value, DateTime.DaysInMonth(now.Year, now.Month));
                    nextDate = new DateTime(now.Year, now.Month, day);
                    if (nextDate <= now)
                    {
                        nextDate = nextDate.AddMonths(1);
                        day = Math.Min(recurrenceDay.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, day);
                    }
                }
                else
                {
                    nextDate = now.AddMonths(1);
                }
                break;

            case "Quarterly":
                nextDate = now.AddMonths(3);
                break;

            case "Annually":
                nextDate = now.AddYears(1);
                break;
        }

        return nextDate;
    }
}



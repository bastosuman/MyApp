using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Core.Mappers;
using MyApp.Data;
using MyApp.Data.Mappers;
using MyApp.Helpers;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(FinancialDbContext context, ILogger<ApplicationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all applications with optional status filter
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationDto>>>> GetApplications([FromQuery] string? status = null)
    {
        try
        {
            var query = _context.Applications
                .Include(a => a.Account)
                .Include(a => a.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var applications = await query
                .ToDtoQuery()
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ApplicationDto>>.SuccessResponse(applications, "Applications retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applications");
            return StatusCode(500, ApiResponse<IEnumerable<ApplicationDto>>.ErrorResponse("An error occurred while retrieving applications"));
        }
    }

    /// <summary>
    /// Get application by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ApplicationDto>>> GetApplication(int id)
    {
        try
        {
            var application = await _context.Applications
                .Include(a => a.Account)
                .Include(a => a.Product)
                .Where(a => a.Id == id)
                .ToDtoQuery()
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound(ApiResponse<ApplicationDto>.ErrorResponse($"Application with ID {id} not found"));
            }

            return Ok(ApiResponse<ApplicationDto>.SuccessResponse(application, "Application retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application {ApplicationId}", id);
            return StatusCode(500, ApiResponse<ApplicationDto>.ErrorResponse("An error occurred while retrieving the application"));
        }
    }

    /// <summary>
    /// Submit a new application
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ApplicationDto>>> CreateApplication(CreateApplicationDto createDto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<ApplicationDto>(this);
            if (validationError != null) return validationError;

            // Verify account exists
            var account = await _context.Accounts.FindAsync(createDto.AccountId);
            if (account == null)
            {
                return BadRequest(ApiResponse<ApplicationDto>.ErrorResponse($"Account with ID {createDto.AccountId} not found"));
            }

            // Verify product exists and is active
            var product = await _context.Products.FindAsync(createDto.ProductId);
            if (product == null)
            {
                return BadRequest(ApiResponse<ApplicationDto>.ErrorResponse($"Product with ID {createDto.ProductId} not found"));
            }

            if (!product.IsActive)
            {
                return BadRequest(ApiResponse<ApplicationDto>.ErrorResponse($"Product '{product.Name}' is not active"));
            }

            // Validate requested amount is within product limits
            if (createDto.RequestedAmount < product.MinAmount || createDto.RequestedAmount > product.MaxAmount)
            {
                return BadRequest(ApiResponse<ApplicationDto>.ErrorResponse(
                    $"Requested amount must be between {product.MinAmount:C} and {product.MaxAmount:C} for this product"));
            }

            var application = new Application
            {
                AccountId = createDto.AccountId,
                ProductId = createDto.ProductId,
                RequestedAmount = createDto.RequestedAmount,
                Status = "Pending",
                ApplicationDate = DateTime.UtcNow,
                Notes = createDto.Notes
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(application)
                .Reference(a => a.Account)
                .LoadAsync();
            await _context.Entry(application)
                .Reference(a => a.Product)
                .LoadAsync();

            var applicationDto = application.ToDto();

            return CreatedAtAction(nameof(GetApplication), new { id = application.Id },
                ApiResponse<ApplicationDto>.SuccessResponse(applicationDto, "Application submitted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application");
            return StatusCode(500, ApiResponse<ApplicationDto>.ErrorResponse("An error occurred while creating the application"));
        }
    }

    /// <summary>
    /// Update application status (approve/reject)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<ApplicationDto>>> UpdateApplicationStatus(int id, UpdateApplicationStatusDto updateDto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<ApplicationDto>(this);
            if (validationError != null) return validationError;

            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Completed" };
            if (!validStatuses.Contains(updateDto.Status))
            {
                return BadRequest(ApiResponse<ApplicationDto>.ErrorResponse(
                    $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}"));
            }

            var application = await _context.Applications
                .Include(a => a.Account)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound(ApiResponse<ApplicationDto>.ErrorResponse($"Application with ID {id} not found"));
            }

            application.Status = updateDto.Status;
            application.DecisionDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(updateDto.Notes))
            {
                application.Notes = updateDto.Notes;
            }

            await _context.SaveChangesAsync();

            var applicationDto = application.ToDto();

            return Ok(ApiResponse<ApplicationDto>.SuccessResponse(applicationDto, "Application status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application status {ApplicationId}", id);
            return StatusCode(500, ApiResponse<ApplicationDto>.ErrorResponse("An error occurred while updating the application status"));
        }
    }

    /// <summary>
    /// Get applications for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationDto>>>> GetApplicationsByAccount(int accountId)
    {
        try
        {
            var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId);
            if (!accountExists)
            {
                return NotFound(ApiResponse<IEnumerable<ApplicationDto>>.ErrorResponse($"Account with ID {accountId} not found"));
            }

            var applications = await _context.Applications
                .Include(a => a.Account)
                .Include(a => a.Product)
                .Where(a => a.AccountId == accountId)
                .ToDtoQuery()
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ApplicationDto>>.SuccessResponse(applications, "Applications retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applications for account {AccountId}", accountId);
            return StatusCode(500, ApiResponse<IEnumerable<ApplicationDto>>.ErrorResponse("An error occurred while retrieving applications"));
        }
    }
}


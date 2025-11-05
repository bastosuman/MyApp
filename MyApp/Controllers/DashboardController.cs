using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Mappers;
using MyApp.Data;
using MyApp.Helpers;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(FinancialDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard summary data
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard()
    {
        try
        {
            var dashboard = new DashboardDto();

            // Get account summary
            var accounts = await _context.Accounts
                .Where(a => a.IsActive)
                .ToListAsync();

            dashboard.AccountSummary = new AccountSummaryDto
            {
                TotalBalance = accounts.Sum(a => a.Balance),
                AccountCount = accounts.Count,
                Accounts = accounts.Select(a => a.ToDto()).ToList()
            };

            // Get recent transactions (last 10)
            var recentTransactions = await _context.Transactions
                .Include(t => t.Account)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    AccountId = t.AccountId,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate,
                    Status = t.Status,
                    AccountNumber = t.Account != null ? t.Account.AccountNumber : null
                })
                .ToListAsync();

            dashboard.RecentTransactions = recentTransactions;

            // Get application status
            var applications = await _context.Applications.ToListAsync();
            dashboard.ApplicationStatus = new ApplicationStatusDto
            {
                Pending = applications.Count(a => a.Status == "Pending"),
                Approved = applications.Count(a => a.Status == "Approved"),
                Rejected = applications.Count(a => a.Status == "Rejected"),
                Total = applications.Count
            };

            // Get available products count
            dashboard.AvailableProductsCount = await _context.Products
                .CountAsync(p => p.IsActive);

            return Ok(ApiResponse<DashboardDto>.SuccessResponse(dashboard, "Dashboard data retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, ApiResponse<DashboardDto>.ErrorResponse("An error occurred while retrieving dashboard data"));
        }
    }
}


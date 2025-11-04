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
public class AccountsController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(FinancialDbContext context, ILogger<AccountsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all accounts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AccountDto>>>> GetAccounts()
    {
        try
        {
            var accounts = (await _context.Accounts.ToListAsync())
                .Select(a => a.ToDto())
                .ToList();

            return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResponse(accounts, "Accounts retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return StatusCode(500, ApiResponse<IEnumerable<AccountDto>>.ErrorResponse("An error occurred while retrieving accounts"));
        }
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccount(int id)
    {
        try
        {
            var accountEntity = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (accountEntity == null)
            {
                return NotFound(ApiResponse<AccountDto>.ErrorResponse($"Account with ID {id} not found"));
            }

            var account = accountEntity.ToDto();

            return Ok(ApiResponse<AccountDto>.SuccessResponse(account, "Account retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {AccountId}", id);
            return StatusCode(500, ApiResponse<AccountDto>.ErrorResponse("An error occurred while retrieving the account"));
        }
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount(CreateAccountDto createDto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<AccountDto>(this);
            if (validationError != null) return validationError;

            // Check if account number already exists
            if (await _context.Accounts.AnyAsync(a => a.AccountNumber == createDto.AccountNumber))
            {
                return BadRequest(ApiResponse<AccountDto>.ErrorResponse($"Account number '{createDto.AccountNumber}' already exists"));
            }

            var account = new Account
            {
                AccountNumber = createDto.AccountNumber,
                AccountHolderName = createDto.AccountHolderName,
                Balance = createDto.Balance,
                AccountType = createDto.AccountType,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var accountDto = account.ToDto();

            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, 
                ApiResponse<AccountDto>.SuccessResponse(accountDto, "Account created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, ApiResponse<AccountDto>.ErrorResponse("An error occurred while creating the account"));
        }
    }

    /// <summary>
    /// Update an existing account
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> UpdateAccount(int id, UpdateAccountDto updateDto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<AccountDto>(this);
            if (validationError != null) return validationError;

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound(ApiResponse<AccountDto>.ErrorResponse($"Account with ID {id} not found"));
            }

            account.AccountHolderName = updateDto.AccountHolderName;
            account.Balance = updateDto.Balance;
            account.AccountType = updateDto.AccountType;
            account.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();

            var accountDto = account.ToDto();

            return Ok(ApiResponse<AccountDto>.SuccessResponse(accountDto, "Account updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {AccountId}", id);
            return StatusCode(500, ApiResponse<AccountDto>.ErrorResponse("An error occurred while updating the account"));
        }
    }

    /// <summary>
    /// Get transactions for a specific account
    /// </summary>
    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetAccountTransactions(int id)
    {
        try
        {
            var accountExists = await _context.Accounts.AnyAsync(a => a.Id == id);
            if (!accountExists)
            {
                return NotFound(ApiResponse<IEnumerable<TransactionDto>>.ErrorResponse($"Account with ID {id} not found"));
            }

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == id)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    AccountId = t.AccountId,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate,
                    Status = t.Status,
                    AccountNumber = t.Account.AccountNumber
                })
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<TransactionDto>>.SuccessResponse(transactions, "Transactions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account {AccountId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<TransactionDto>>.ErrorResponse("An error occurred while retrieving transactions"));
        }
    }

    /// <summary>
    /// Get applications for a specific account
    /// </summary>
    [HttpGet("{id}/applications")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationDto>>>> GetAccountApplications(int id)
    {
        try
        {
            var accountExists = await _context.Accounts.AnyAsync(a => a.Id == id);
            if (!accountExists)
            {
                return NotFound(ApiResponse<IEnumerable<ApplicationDto>>.ErrorResponse($"Account with ID {id} not found"));
            }

            var applications = await _context.Applications
                .Include(a => a.Account)
                .Include(a => a.Product)
                .Where(a => a.AccountId == id)
                .ToDtoQuery()
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ApplicationDto>>.SuccessResponse(applications, "Applications retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applications for account {AccountId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<ApplicationDto>>.ErrorResponse("An error occurred while retrieving applications"));
        }
    }
}


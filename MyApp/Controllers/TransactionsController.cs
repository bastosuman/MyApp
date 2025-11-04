using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Helpers;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(FinancialDbContext context, ILogger<TransactionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all transactions with optional pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var transactions = await _context.Transactions
                .Include(t => t.Account)
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<TransactionDto>>.SuccessResponse(transactions, "Transactions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, ApiResponse<IEnumerable<TransactionDto>>.ErrorResponse("An error occurred while retrieving transactions"));
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(int id)
    {
        try
        {
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Id == id)
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
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound(ApiResponse<TransactionDto>.ErrorResponse($"Transaction with ID {id} not found"));
            }

            return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, "Transaction retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, ApiResponse<TransactionDto>.ErrorResponse("An error occurred while retrieving the transaction"));
        }
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(CreateTransactionDto createDto)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<TransactionDto>(this);
            if (validationError != null) return validationError;

            // Verify account exists
            var account = await _context.Accounts.FindAsync(createDto.AccountId);
            if (account == null)
            {
                return BadRequest(ApiResponse<TransactionDto>.ErrorResponse($"Account with ID {createDto.AccountId} not found"));
            }

            // Validate transaction type
            var validTransactionTypes = new[] { "Deposit", "Withdrawal", "Transfer" };
            if (!validTransactionTypes.Contains(createDto.TransactionType))
            {
                return BadRequest(ApiResponse<TransactionDto>.ErrorResponse($"Invalid transaction type. Must be one of: {string.Join(", ", validTransactionTypes)}"));
            }

            // Validate amount
            if (createDto.Amount <= 0)
            {
                return BadRequest(ApiResponse<TransactionDto>.ErrorResponse("Transaction amount must be greater than zero"));
            }

            // Update account balance based on transaction type
            if (createDto.TransactionType == "Deposit")
            {
                account.Balance += createDto.Amount;
            }
            else if (createDto.TransactionType == "Withdrawal")
            {
                if (account.Balance < createDto.Amount)
                {
                    return BadRequest(ApiResponse<TransactionDto>.ErrorResponse("Insufficient balance for withdrawal"));
                }
                account.Balance -= createDto.Amount;
            }

            var transaction = new Transaction
            {
                AccountId = createDto.AccountId,
                TransactionType = createDto.TransactionType,
                Amount = createDto.Amount,
                Description = createDto.Description,
                TransactionDate = createDto.TransactionDate,
                Status = "Completed"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var transactionDto = new TransactionDto
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate,
                Status = transaction.Status,
                AccountNumber = account.AccountNumber
            };

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id },
                ApiResponse<TransactionDto>.SuccessResponse(transactionDto, "Transaction created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, ApiResponse<TransactionDto>.ErrorResponse("An error occurred while creating the transaction"));
        }
    }

    /// <summary>
    /// Get transactions for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTransactionsByAccount(int accountId)
    {
        try
        {
            var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId);
            if (!accountExists)
            {
                return NotFound(ApiResponse<IEnumerable<TransactionDto>>.ErrorResponse($"Account with ID {accountId} not found"));
            }

            var transactions = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
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
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<TransactionDto>>.SuccessResponse(transactions, "Transactions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account {AccountId}", accountId);
            return StatusCode(500, ApiResponse<IEnumerable<TransactionDto>>.ErrorResponse("An error occurred while retrieving transactions"));
        }
    }
}


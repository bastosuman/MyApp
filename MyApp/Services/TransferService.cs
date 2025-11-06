using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Services;

public class TransferService
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<TransferService> _logger;

    public TransferService(FinancialDbContext context, ILogger<TransferService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Validates a transfer before execution
    /// </summary>
    public async Task<TransferValidationResult> ValidateTransferAsync(
        int sourceAccountId, 
        int? destinationAccountId, 
        string? destinationAccountNumber,
        decimal amount,
        string transferType)
    {
        var result = new TransferValidationResult { IsValid = true };

        // Validate source account
        var sourceAccount = await _context.Accounts
            .Include(a => a.Limits)
            .FirstOrDefaultAsync(a => a.Id == sourceAccountId);

        if (sourceAccount == null)
        {
            result.IsValid = false;
            result.ErrorMessage = "Source account not found";
            return result;
        }

        if (!sourceAccount.IsActive)
        {
            result.IsValid = false;
            result.ErrorMessage = "Source account is not active";
            return result;
        }

        // Validate amount
        if (amount <= 0)
        {
            result.IsValid = false;
            result.ErrorMessage = "Transfer amount must be greater than zero";
            return result;
        }

        // Validate destination account
        Account? destinationAccount = null;
        if (transferType == "Internal" && destinationAccountId.HasValue)
        {
            destinationAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == destinationAccountId.Value);

            if (destinationAccount == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Destination account not found";
                return result;
            }

            if (!destinationAccount.IsActive)
            {
                result.IsValid = false;
                result.ErrorMessage = "Destination account is not active";
                return result;
            }

            if (sourceAccountId == destinationAccountId.Value)
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannot transfer to the same account";
                return result;
            }
        }
        else if (transferType == "External" && !string.IsNullOrWhiteSpace(destinationAccountNumber))
        {
            destinationAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == destinationAccountNumber);

            if (destinationAccount == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Destination account not found";
                return result;
            }

            if (!destinationAccount.IsActive)
            {
                result.IsValid = false;
                result.ErrorMessage = "Destination account is not active";
                return result;
            }

            if (sourceAccount.AccountNumber == destinationAccountNumber)
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannot transfer to the same account";
                return result;
            }
        }

        // Check balance
        if (sourceAccount.Balance < amount)
        {
            result.IsValid = false;
            result.ErrorMessage = "Insufficient balance for transfer";
            return result;
        }

        // Check transfer limits
        var limitsResult = await CheckTransferLimitsAsync(sourceAccountId, amount);
        if (!limitsResult.IsValid)
        {
            result.IsValid = false;
            result.ErrorMessage = limitsResult.ErrorMessage;
            return result;
        }

        result.SourceAccount = sourceAccount;
        result.DestinationAccount = destinationAccount;
        return result;
    }

    /// <summary>
    /// Checks if transfer is within account limits
    /// </summary>
    public async Task<TransferValidationResult> CheckTransferLimitsAsync(int accountId, decimal amount)
    {
        var result = new TransferValidationResult { IsValid = true };

        var account = await _context.Accounts
            .Include(a => a.Limits)
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null)
        {
            result.IsValid = false;
            result.ErrorMessage = "Account not found";
            return result;
        }

        // Get or create limits
        var limits = account.Limits;
        if (limits == null)
        {
            limits = new AccountLimits
            {
                AccountId = accountId,
                DailyTransferLimit = 10000m,
                MonthlyTransferLimit = 50000m,
                PerTransactionMax = 5000m,
                PerTransactionMin = 1m
            };
            _context.AccountLimits.Add(limits);
            await _context.SaveChangesAsync();
        }

        // Reset daily limit if needed
        if (limits.LastDailyReset == null || limits.LastDailyReset.Value.Date < DateTime.UtcNow.Date)
        {
            limits.DailyTransferUsed = 0;
            limits.LastDailyReset = DateTime.UtcNow.Date;
            await _context.SaveChangesAsync();
        }

        // Reset monthly limit if needed
        if (limits.LastMonthlyReset == null || 
            limits.LastMonthlyReset.Value.Year != DateTime.UtcNow.Year ||
            limits.LastMonthlyReset.Value.Month != DateTime.UtcNow.Month)
        {
            limits.MonthlyTransferUsed = 0;
            limits.LastMonthlyReset = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            await _context.SaveChangesAsync();
        }

        // Check per-transaction limits
        if (amount < limits.PerTransactionMin)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Transfer amount must be at least {limits.PerTransactionMin:C}";
            return result;
        }

        if (amount > limits.PerTransactionMax)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Transfer amount cannot exceed {limits.PerTransactionMax:C} per transaction";
            return result;
        }

        // Check daily limit
        var dailyAvailable = limits.DailyTransferLimit - limits.DailyTransferUsed;
        if (amount > dailyAvailable)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Daily transfer limit of {limits.DailyTransferLimit:C} would be exceeded. Available: {dailyAvailable:C}";
            return result;
        }

        // Check monthly limit
        var monthlyAvailable = limits.MonthlyTransferLimit - limits.MonthlyTransferUsed;
        if (amount > monthlyAvailable)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Monthly transfer limit of {limits.MonthlyTransferLimit:C} would be exceeded. Available: {monthlyAvailable:C}";
            return result;
        }

        return result;
    }

    /// <summary>
    /// Executes an internal transfer between accounts
    /// </summary>
    public async Task<TransferExecutionResult> ExecuteInternalTransferAsync(CreateInternalTransferDto dto)
    {
        // Use database transaction if supported, otherwise rely on SaveChanges atomicity
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        try
        {
            var providerName = _context.Database.ProviderName;
            if (providerName != null && !providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }
        }
        catch
        {
            // In-memory database doesn't support transactions, continue without
        }
        try
        {
            // Validate transfer
            var validation = await ValidateTransferAsync(
                dto.SourceAccountId,
                dto.DestinationAccountId,
                null,
                dto.Amount,
                "Internal");

            if (!validation.IsValid)
            {
                return new TransferExecutionResult
                {
                    Success = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            var sourceAccount = validation.SourceAccount!;
            var destinationAccount = validation.DestinationAccount!;

            // Create transfer record
            var transfer = new Transfer
            {
                SourceAccountId = dto.SourceAccountId,
                DestinationAccountId = dto.DestinationAccountId,
                TransferType = "Internal",
                Amount = dto.Amount,
                Description = dto.Description,
                Status = "Processing",
                TransferDate = dto.ScheduledDate ?? DateTime.UtcNow,
                ScheduledDate = dto.ScheduledDate,
                CreatedDate = DateTime.UtcNow
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            // Update balances
            sourceAccount.Balance -= dto.Amount;
            destinationAccount.Balance += dto.Amount;

            // Create source transaction
            var sourceTransaction = new Transaction
            {
                AccountId = sourceAccount.Id,
                TransactionType = "Transfer Out",
                Amount = dto.Amount,
                Description = $"Transfer to {destinationAccount.AccountNumber}: {dto.Description}",
                TransactionDate = transfer.TransferDate,
                Status = "Completed"
            };

            // Create destination transaction
            var destinationTransaction = new Transaction
            {
                AccountId = destinationAccount.Id,
                TransactionType = "Transfer In",
                Amount = dto.Amount,
                Description = $"Transfer from {sourceAccount.AccountNumber}: {dto.Description}",
                TransactionDate = transfer.TransferDate,
                Status = "Completed"
            };

            _context.Transactions.AddRange(sourceTransaction, destinationTransaction);
            await _context.SaveChangesAsync();

            // Link transactions to transfer
            transfer.SourceTransactionId = sourceTransaction.Id;
            transfer.DestinationTransactionId = destinationTransaction.Id;
            transfer.Status = "Completed";
            transfer.CompletedDate = DateTime.UtcNow;

            // Update transfer limits
            await UpdateTransferLimitsAsync(sourceAccount.Id, dto.Amount);

            await _context.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return new TransferExecutionResult
            {
                Success = true,
                TransferId = transfer.Id,
                Message = "Transfer completed successfully"
            };
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                    // Ignore rollback errors
                }
            }
            _logger.LogError(ex, "Error executing internal transfer");
            return new TransferExecutionResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the transfer"
            };
        }
        finally
        {
            if (transaction != null)
            {
                try
                {
                    await transaction.DisposeAsync();
                }
                catch
                {
                    // Ignore dispose errors
                }
            }
        }
    }

    /// <summary>
    /// Executes an external transfer to another account
    /// </summary>
    public async Task<TransferExecutionResult> ExecuteExternalTransferAsync(CreateExternalTransferDto dto)
    {
        // Use database transaction if supported, otherwise rely on SaveChanges atomicity
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        try
        {
            var providerName = _context.Database.ProviderName;
            if (providerName != null && !providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }
        }
        catch
        {
            // In-memory database doesn't support transactions, continue without
        }
        try
        {
            // Find destination account by account number
            var destinationAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == dto.DestinationAccountNumber);

            if (destinationAccount == null)
            {
                return new TransferExecutionResult
                {
                    Success = false,
                    ErrorMessage = "Destination account not found"
                };
            }

            // Validate transfer
            var validation = await ValidateTransferAsync(
                dto.SourceAccountId,
                destinationAccount.Id,
                dto.DestinationAccountNumber,
                dto.Amount,
                "External");

            if (!validation.IsValid)
            {
                return new TransferExecutionResult
                {
                    Success = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            var sourceAccount = validation.SourceAccount!;

            // Create transfer record
            var transfer = new Transfer
            {
                SourceAccountId = dto.SourceAccountId,
                DestinationAccountId = destinationAccount.Id,
                DestinationAccountNumber = dto.DestinationAccountNumber,
                TransferType = "External",
                Amount = dto.Amount,
                Description = dto.Description,
                Status = "Processing",
                TransferDate = dto.ScheduledDate ?? DateTime.UtcNow,
                ScheduledDate = dto.ScheduledDate,
                CreatedDate = DateTime.UtcNow
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            // Update balances
            sourceAccount.Balance -= dto.Amount;
            destinationAccount.Balance += dto.Amount;

            // Create source transaction
            var sourceTransaction = new Transaction
            {
                AccountId = sourceAccount.Id,
                TransactionType = "Transfer Out",
                Amount = dto.Amount,
                Description = $"Transfer to {dto.DestinationAccountNumber}: {dto.Description}",
                TransactionDate = transfer.TransferDate,
                Status = "Completed"
            };

            // Create destination transaction
            var destinationTransaction = new Transaction
            {
                AccountId = destinationAccount.Id,
                TransactionType = "Transfer In",
                Amount = dto.Amount,
                Description = $"Transfer from {sourceAccount.AccountNumber}: {dto.Description}",
                TransactionDate = transfer.TransferDate,
                Status = "Completed"
            };

            _context.Transactions.AddRange(sourceTransaction, destinationTransaction);
            await _context.SaveChangesAsync();

            // Link transactions to transfer
            transfer.SourceTransactionId = sourceTransaction.Id;
            transfer.DestinationTransactionId = destinationTransaction.Id;
            transfer.Status = "Completed";
            transfer.CompletedDate = DateTime.UtcNow;

            // Update transfer limits
            await UpdateTransferLimitsAsync(sourceAccount.Id, dto.Amount);

            await _context.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return new TransferExecutionResult
            {
                Success = true,
                TransferId = transfer.Id,
                Message = "Transfer completed successfully"
            };
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                    // Ignore rollback errors
                }
            }
            _logger.LogError(ex, "Error executing external transfer");
            return new TransferExecutionResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the transfer"
            };
        }
        finally
        {
            if (transaction != null)
            {
                try
                {
                    await transaction.DisposeAsync();
                }
                catch
                {
                    // Ignore dispose errors
                }
            }
        }
    }

    /// <summary>
    /// Updates transfer limits after a successful transfer
    /// </summary>
    private async Task UpdateTransferLimitsAsync(int accountId, decimal amount)
    {
        var account = await _context.Accounts
            .Include(a => a.Limits)
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account?.Limits == null) return;

        var limits = account.Limits;

        // Reset daily limit if needed
        if (limits.LastDailyReset == null || limits.LastDailyReset.Value.Date < DateTime.UtcNow.Date)
        {
            limits.DailyTransferUsed = 0;
            limits.LastDailyReset = DateTime.UtcNow.Date;
        }

        // Reset monthly limit if needed
        if (limits.LastMonthlyReset == null ||
            limits.LastMonthlyReset.Value.Year != DateTime.UtcNow.Year ||
            limits.LastMonthlyReset.Value.Month != DateTime.UtcNow.Month)
        {
            limits.MonthlyTransferUsed = 0;
            limits.LastMonthlyReset = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        }

        limits.DailyTransferUsed += amount;
        limits.MonthlyTransferUsed += amount;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Cancels a pending transfer
    /// </summary>
    public async Task<bool> CancelTransferAsync(int transferId)
    {
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == transferId);

        if (transfer == null)
        {
            return false;
        }

        if (transfer.Status != "Pending" && transfer.Status != "Processing")
        {
            return false; // Can only cancel pending or processing transfers
        }

        transfer.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return true;
    }
}

// Helper classes for service results
public class TransferValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Account? SourceAccount { get; set; }
    public Account? DestinationAccount { get; set; }
}

public class TransferExecutionResult
{
    public bool Success { get; set; }
    public int? TransferId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Services;

/// <summary>
/// Helper class to reduce duplication in transfer execution logic
/// </summary>
internal static class TransferExecutionHelper
{
    /// <summary>
    /// Executes a transfer with common logic for both internal and external transfers
    /// </summary>
    public static async Task<TransferExecutionResult> ExecuteTransferAsync(
        FinancialDbContext context,
        ILogger logger,
        Account sourceAccount,
        Account destinationAccount,
        decimal amount,
        string description,
        string transferType,
        DateTime? scheduledDate,
        string? destinationAccountNumber,
        Func<Account, Account, TransferValidationResult, Task<TransferValidationResult>> validateTransfer)
    {
        // Use database transaction if supported
        IDbContextTransaction? transaction = null;
        try
        {
            var providerName = context.Database.ProviderName;
            if (providerName != null && !providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                transaction = await context.Database.BeginTransactionAsync();
            }
        }
        catch
        {
            // In-memory database doesn't support transactions, continue without
        }

        try
        {
            // Validate transfer
            var validation = await validateTransfer(sourceAccount, destinationAccount, new TransferValidationResult { IsValid = true });
            if (!validation.IsValid)
            {
                return new TransferExecutionResult
                {
                    Success = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            var transferDate = scheduledDate ?? DateTime.UtcNow;

            // Create transfer record
            var transfer = new Transfer
            {
                SourceAccountId = sourceAccount.Id,
                DestinationAccountId = destinationAccount.Id,
                DestinationAccountNumber = destinationAccountNumber,
                TransferType = transferType,
                Amount = amount,
                Description = description,
                Status = "Processing",
                TransferDate = transferDate,
                ScheduledDate = scheduledDate,
                CreatedDate = DateTime.UtcNow
            };

            context.Transfers.Add(transfer);
            await context.SaveChangesAsync();

            // Update balances
            sourceAccount.Balance -= amount;
            destinationAccount.Balance += amount;

            // Create transactions
            var (sourceTransaction, destinationTransaction) = CreateTransferTransactions(
                sourceAccount,
                destinationAccount,
                amount,
                description,
                transferDate,
                destinationAccountNumber);

            context.Transactions.AddRange(sourceTransaction, destinationTransaction);
            await context.SaveChangesAsync();

            // Link transactions to transfer
            transfer.SourceTransactionId = sourceTransaction.Id;
            transfer.DestinationTransactionId = destinationTransaction.Id;
            transfer.Status = "Completed";
            transfer.CompletedDate = DateTime.UtcNow;

            // Update transfer limits
            await UpdateTransferLimitsAsync(context, sourceAccount.Id, amount);

            await context.SaveChangesAsync();
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
            logger.LogError(ex, "Error executing {TransferType} transfer", transferType);
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
    /// Creates source and destination transactions for a transfer
    /// </summary>
    private static (Transaction sourceTransaction, Transaction destinationTransaction) CreateTransferTransactions(
        Account sourceAccount,
        Account destinationAccount,
        decimal amount,
        string description,
        DateTime transferDate,
        string? destinationAccountNumber)
    {
        var destAccountIdentifier = destinationAccountNumber ?? destinationAccount.AccountNumber;

        var sourceTransaction = new Transaction
        {
            AccountId = sourceAccount.Id,
            TransactionType = "Transfer Out",
            Amount = amount,
            Description = $"Transfer to {destAccountIdentifier}: {description}",
            TransactionDate = transferDate,
            Status = "Completed"
        };

        var destinationTransaction = new Transaction
        {
            AccountId = destinationAccount.Id,
            TransactionType = "Transfer In",
            Amount = amount,
            Description = $"Transfer from {sourceAccount.AccountNumber}: {description}",
            TransactionDate = transferDate,
            Status = "Completed"
        };

        return (sourceTransaction, destinationTransaction);
    }

    /// <summary>
    /// Updates transfer limits after a successful transfer
    /// </summary>
    private static async Task UpdateTransferLimitsAsync(FinancialDbContext context, int accountId, decimal amount)
    {
        var account = await context.Accounts
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

        await context.SaveChangesAsync();
    }
}


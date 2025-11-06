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
        ILogger logger,
        TransferExecutionParameters parameters)
    {
        // Use database transaction if supported
        IDbContextTransaction? transaction = null;
        try
        {
            var providerName = parameters.Context.Database.ProviderName;
            if (providerName != null && !providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                transaction = await parameters.Context.Database.BeginTransactionAsync();
            }
        }
        catch
        {
            // In-memory database doesn't support transactions, continue without
        }

        try
        {
            // Validate transfer
            if (!parameters.Validation.IsValid)
            {
                return new TransferExecutionResult
                {
                    Success = false,
                    ErrorMessage = parameters.Validation.ErrorMessage
                };
            }

            var transferDate = parameters.ScheduledDate ?? DateTime.UtcNow;

            // Create transfer record
            var transfer = new Transfer
            {
                SourceAccountId = parameters.SourceAccount.Id,
                DestinationAccountId = parameters.DestinationAccount.Id,
                DestinationAccountNumber = parameters.DestinationAccountNumber,
                TransferType = parameters.TransferType,
                Amount = parameters.Amount,
                Description = parameters.Description,
                Status = "Processing",
                TransferDate = transferDate,
                ScheduledDate = parameters.ScheduledDate,
                CreatedDate = DateTime.UtcNow
            };

            parameters.Context.Transfers.Add(transfer);
            await parameters.Context.SaveChangesAsync();

            // Update balances
            parameters.SourceAccount.Balance -= parameters.Amount;
            parameters.DestinationAccount.Balance += parameters.Amount;

            // Create transactions
            var (sourceTransaction, destinationTransaction) = CreateTransferTransactions(
                parameters.SourceAccount,
                parameters.DestinationAccount,
                parameters.Amount,
                parameters.Description,
                transferDate,
                parameters.DestinationAccountNumber);

            parameters.Context.Transactions.AddRange(sourceTransaction, destinationTransaction);
            await parameters.Context.SaveChangesAsync();

            // Link transactions to transfer
            transfer.SourceTransactionId = sourceTransaction.Id;
            transfer.DestinationTransactionId = destinationTransaction.Id;
            transfer.Status = "Completed";
            transfer.CompletedDate = DateTime.UtcNow;

            // Update transfer limits
            await UpdateTransferLimitsAsync(parameters.Context, parameters.SourceAccount.Id, parameters.Amount);

            await parameters.Context.SaveChangesAsync();
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
            logger.LogError(ex, "Error executing {TransferType} transfer", parameters.TransferType);
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
            limits.LastMonthlyReset = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        limits.DailyTransferUsed += amount;
        limits.MonthlyTransferUsed += amount;

        await context.SaveChangesAsync();
    }
}


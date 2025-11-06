using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Services;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ServicesTests;

public class TransferServiceTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    private static ILogger<TransferService> CreateLogger()
    {
        return new LoggerFactory().CreateLogger<TransferService>();
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnValid_WhenAllConditionsMet()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        var destinationAccount = TestDataFactory.CreateTestAccount("ACC002", "John Doe", 1000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destinationAccount);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ValidateTransferAsync(
            sourceAccount.Id,
            destinationAccount.Id,
            null,
            1000m,
            "Internal");

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.SourceAccount);
        Assert.NotNull(result.DestinationAccount);
        Assert.Equal(sourceAccount.Id, result.SourceAccount.Id);
        Assert.Equal(destinationAccount.Id, result.DestinationAccount.Id);
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnInvalid_WhenSourceAccountNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        // Act
        var result = await service.ValidateTransferAsync(999, null, null, 1000m, "Internal");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Source account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnInvalid_WhenSourceAccountInactive()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: false);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ValidateTransferAsync(sourceAccount.Id, null, null, 1000m, "Internal");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Source account is not active", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnInvalid_WhenAmountIsZero()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ValidateTransferAsync(sourceAccount.Id, null, null, 0m, "Internal");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Transfer amount must be greater than zero", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnInvalid_WhenDestinationAccountNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ValidateTransferAsync(sourceAccount.Id, 999, null, 1000m, "Internal");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Destination account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnInvalid_WhenTransferringToSameAccount()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ValidateTransferAsync(account.Id, account.Id, null, 1000m, "Internal");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Cannot transfer to the same account", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateTransferAsync_ShouldReturnInvalid_WhenInsufficientBalance()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 500m, isActive: true);
        var destinationAccount = TestDataFactory.CreateTestAccount("ACC002", "John Doe", 1000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destinationAccount);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ValidateTransferAsync(sourceAccount.Id, destinationAccount.Id, null, 1000m, "Internal");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Insufficient balance", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldReturnValid_WhenWithinLimits()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CheckTransferLimitsAsync(account.Id, 1000m);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldReturnInvalid_WhenExceedsPerTransactionMax()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 50000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Create limits with lower max
        var limits = new AccountLimits
        {
            AccountId = account.Id,
            PerTransactionMax = 5000m,
            PerTransactionMin = 1m,
            DailyTransferLimit = 10000m,
            MonthlyTransferLimit = 50000m
        };
        context.AccountLimits.Add(limits);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CheckTransferLimitsAsync(account.Id, 6000m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("cannot exceed", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldReturnInvalid_WhenBelowPerTransactionMin()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CheckTransferLimitsAsync(account.Id, 0.50m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be at least", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldResetDailyLimit_WhenNewDay()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 50000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var limits = new AccountLimits
        {
            AccountId = account.Id,
            DailyTransferLimit = 10000m,
            MonthlyTransferLimit = 50000m,
            PerTransactionMax = 5000m,
            PerTransactionMin = 1m,
            DailyTransferUsed = 9000m,
            LastDailyReset = DateTime.UtcNow.AddDays(-1).Date
        };
        context.AccountLimits.Add(limits);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CheckTransferLimitsAsync(account.Id, 2000m);

        // Assert
        Assert.True(result.IsValid);
        // Reload limits from database
        var updatedLimits = await context.AccountLimits
            .FirstOrDefaultAsync(l => l.AccountId == account.Id);
        Assert.NotNull(updatedLimits);
        Assert.Equal(0m, updatedLimits.DailyTransferUsed); // Should be reset
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldResetMonthlyLimit_WhenNewMonth()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 50000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        var limits = new AccountLimits
        {
            AccountId = account.Id,
            DailyTransferLimit = 10000m,
            MonthlyTransferLimit = 50000m,
            PerTransactionMax = 5000m,
            PerTransactionMin = 1m,
            MonthlyTransferUsed = 40000m,
            LastMonthlyReset = new DateTime(lastMonth.Year, lastMonth.Month, 1)
        };
        context.AccountLimits.Add(limits);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CheckTransferLimitsAsync(account.Id, 2000m);

        // Assert
        Assert.True(result.IsValid);
        // Reload limits from database
        var updatedLimits = await context.AccountLimits
            .FirstOrDefaultAsync(l => l.AccountId == account.Id);
        Assert.NotNull(updatedLimits);
        Assert.Equal(0m, updatedLimits.MonthlyTransferUsed); // Should be reset
    }

    [Fact]
    public async Task ExecuteInternalTransferAsync_ShouldTransferFunds_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        var destinationAccount = TestDataFactory.CreateTestAccount("ACC002", "John Doe", 1000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destinationAccount);
        await context.SaveChangesAsync();

        var dto = new CreateInternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountId = destinationAccount.Id,
            Amount = 1000m,
            Description = "Test transfer"
        };

        // Act
        var result = await service.ExecuteInternalTransferAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.TransferId);

        // Verify balances
        var updatedSource = await context.Accounts.FindAsync(sourceAccount.Id);
        var updatedDest = await context.Accounts.FindAsync(destinationAccount.Id);
        Assert.NotNull(updatedSource);
        Assert.NotNull(updatedDest);
        Assert.Equal(4000m, updatedSource.Balance);
        Assert.Equal(2000m, updatedDest.Balance);

        // Verify transactions created
        var transactions = await context.Transactions
            .Where(t => t.AccountId == sourceAccount.Id || t.AccountId == destinationAccount.Id)
            .ToListAsync();
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public async Task ExecuteInternalTransferAsync_ShouldRollback_WhenErrorOccurs()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 500m, isActive: true);
        var destinationAccount = TestDataFactory.CreateTestAccount("ACC002", "John Doe", 1000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destinationAccount);
        await context.SaveChangesAsync();

        var dto = new CreateInternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountId = destinationAccount.Id,
            Amount = 1000m, // More than source balance
            Description = "Test transfer"
        };

        // Act
        var result = await service.ExecuteInternalTransferAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient balance", result.ErrorMessage);

        // Verify balances unchanged
        var updatedSource = await context.Accounts.FindAsync(sourceAccount.Id);
        var updatedDest = await context.Accounts.FindAsync(destinationAccount.Id);
        Assert.NotNull(updatedSource);
        Assert.NotNull(updatedDest);
        Assert.Equal(500m, updatedSource.Balance);
        Assert.Equal(1000m, updatedDest.Balance);
    }

    [Fact]
    public async Task ExecuteExternalTransferAsync_ShouldTransferFunds_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        var destinationAccount = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 1000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destinationAccount);
        await context.SaveChangesAsync();

        var dto = new CreateExternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountNumber = destinationAccount.AccountNumber,
            Amount = 1000m,
            Description = "External transfer"
        };

        // Act
        var result = await service.ExecuteExternalTransferAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.TransferId);

        // Verify balances
        var updatedSource = await context.Accounts.FindAsync(sourceAccount.Id);
        var updatedDest = await context.Accounts.FindAsync(destinationAccount.Id);
        Assert.NotNull(updatedSource);
        Assert.NotNull(updatedDest);
        Assert.Equal(4000m, updatedSource.Balance);
        Assert.Equal(2000m, updatedDest.Balance);
    }

    [Fact]
    public async Task ExecuteExternalTransferAsync_ShouldReturnError_WhenAccountNumberNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        var dto = new CreateExternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountNumber = "NONEXISTENT",
            Amount = 1000m,
            Description = "External transfer"
        };

        // Act
        var result = await service.ExecuteExternalTransferAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Destination account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task CancelTransferAsync_ShouldCancel_WhenTransferIsPending()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = sourceAccount.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Pending",
            TransferDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CancelTransferAsync(transfer.Id);

        // Assert
        Assert.True(result);
        var updatedTransfer = await context.Transfers.FindAsync(transfer.Id);
        Assert.NotNull(updatedTransfer);
        Assert.Equal("Cancelled", updatedTransfer.Status);
    }

    [Fact]
    public async Task CancelTransferAsync_ShouldReturnFalse_WhenTransferIsCompleted()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = sourceAccount.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CancelTransferAsync(transfer.Id);

        // Assert
        Assert.False(result);
        var updatedTransfer = await context.Transfers.FindAsync(transfer.Id);
        Assert.NotNull(updatedTransfer);
        Assert.Equal("Completed", updatedTransfer.Status); // Should remain completed
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldReturnInvalid_WhenExceedsDailyLimit()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 50000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Create limits with daily used already at 9000 (near the limit)
        // The reset logic will check if LastDailyReset < today, so we set it to today to prevent reset
        var limits = new AccountLimits
        {
            AccountId = account.Id,
            DailyTransferLimit = 10000m,
            MonthlyTransferLimit = 50000m,
            PerTransactionMax = 5000m, // This is higher than 2000, so it won't block
            PerTransactionMin = 1m,
            DailyTransferUsed = 9000m, // Already used 9000, only 1000 left
            MonthlyTransferUsed = 0m,
            LastDailyReset = DateTime.UtcNow.Date, // Today, so reset won't happen
            LastMonthlyReset = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
        };
        context.AccountLimits.Add(limits);
        account.Limits = limits; // Set the navigation property
        await context.SaveChangesAsync();

        // Reload to ensure limits are attached
        context.Entry(account).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        account = await context.Accounts.Include(a => a.Limits).FirstAsync(a => a.Id == account.Id);
        
        // Act - Try to transfer 2000, but only 1000 is available (10000 - 9000)
        var result = await service.CheckTransferLimitsAsync(account.Id, 2000m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Daily transfer limit", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckTransferLimitsAsync_ShouldReturnInvalid_WhenExceedsMonthlyLimit()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var service = new TransferService(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 50000m, isActive: true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Set up limits where monthly limit is the constraint
        // PerTransactionMax must be high enough to not block, daily limit must also not block
        var limits = new AccountLimits
        {
            AccountId = account.Id,
            DailyTransferLimit = 10000m, // High enough to not block 6000
            MonthlyTransferLimit = 50000m,
            PerTransactionMax = 10000m, // High enough to not block 6000
            PerTransactionMin = 1m,
            DailyTransferUsed = 0m, // No daily usage
            MonthlyTransferUsed = 45000m, // Already used 45000, only 5000 left
            LastDailyReset = DateTime.UtcNow.Date, // Today, so reset won't happen
            LastMonthlyReset = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1) // Same month, so reset won't happen
        };
        context.AccountLimits.Add(limits);
        account.Limits = limits; // Set the navigation property
        await context.SaveChangesAsync();

        // Reload to ensure limits are attached
        context.Entry(account).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        account = await context.Accounts.Include(a => a.Limits).FirstAsync(a => a.Id == account.Id);

        // Act - Try to transfer 6000, but only 5000 is available monthly (50000 - 45000)
        var result = await service.CheckTransferLimitsAsync(account.Id, 6000m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Monthly transfer limit", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}


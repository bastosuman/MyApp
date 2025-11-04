using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class TransactionsControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnTransactions_WithPagination()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Create 10 transactions
        for (int i = 0; i < 10; i++)
        {
            var transaction = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 100m + i);
            context.Transactions.Add(transaction);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransactions(page: 1, pageSize: 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransactionDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var transactions = response.Data.ToList();
        Assert.Equal(5, transactions.Count);
    }

    [Fact]
    public async Task GetTransaction_ShouldReturnTransaction_WhenExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 500m);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransaction(transaction.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(transaction.TransactionType, response.Data.TransactionType);
        Assert.Equal(transaction.Amount, response.Data.Amount);
    }

    [Fact]
    public async Task CreateTransaction_ShouldCreateDeposit_AndUpdateAccountBalance()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount(balance: 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            AccountId = account.Id,
            TransactionType = "Deposit",
            Amount = 500m,
            Description = "Test deposit"
        };

        // Act
        var result = await controller.CreateTransaction(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(createdResult.Value);
        Assert.True(response.Success);

        // Verify account balance was updated
        var updatedAccount = await context.Accounts.FindAsync(account.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(1500m, updatedAccount.Balance); // 1000 + 500
    }

    [Fact]
    public async Task CreateTransaction_ShouldCreateWithdrawal_AndUpdateAccountBalance()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount(balance: 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            AccountId = account.Id,
            TransactionType = "Withdrawal",
            Amount = 300m,
            Description = "Test withdrawal"
        };

        // Act
        var result = await controller.CreateTransaction(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(createdResult.Value);
        Assert.True(response.Success);

        // Verify account balance was updated
        var updatedAccount = await context.Accounts.FindAsync(account.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(700m, updatedAccount.Balance); // 1000 - 300
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnBadRequest_WhenInsufficientBalance()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount(balance: 100m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            AccountId = account.Id,
            TransactionType = "Withdrawal",
            Amount = 500m, // More than balance
            Description = "Test withdrawal"
        };

        // Act
        var result = await controller.CreateTransaction(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Insufficient balance", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnBadRequest_WhenInvalidTransactionType()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var createDto = new CreateTransactionDto
        {
            AccountId = account.Id,
            TransactionType = "InvalidType",
            Amount = 500m,
            Description = "Test"
        };

        // Act
        var result = await controller.CreateTransaction(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetTransactionsByAccount_ShouldReturnTransactions_ForAccount()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001");
        var account2 = TestDataFactory.CreateTestAccount("ACC002");
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transaction1 = TestDataFactory.CreateTestTransaction(account1.Id, "Deposit", 500m);
        var transaction2 = TestDataFactory.CreateTestTransaction(account1.Id, "Withdrawal", 200m);
        var transaction3 = TestDataFactory.CreateTestTransaction(account2.Id, "Deposit", 1000m);
        context.Transactions.AddRange(transaction1, transaction2, transaction3);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransactionsByAccount(account1.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransactionDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var transactions = response.Data.ToList();
        Assert.Equal(2, transactions.Count);
        Assert.All(transactions, t => Assert.Equal(account1.Id, t.AccountId));
    }
}


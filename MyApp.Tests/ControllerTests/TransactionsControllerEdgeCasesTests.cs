using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class TransactionsControllerEdgeCasesTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnBadRequest_WhenAmountIsZero()
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
            TransactionType = "Deposit",
            Amount = 0m, // Zero amount
            Description = "Test"
        };

        // Act
        var result = await controller.CreateTransaction(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("greater than zero", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnBadRequest_WhenAmountIsNegative()
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
            TransactionType = "Deposit",
            Amount = -100m, // Negative amount
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
    public async Task GetTransaction_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        // Act
        var result = await controller.GetTransaction(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetTransactions_ShouldHandleInvalidPageNumbers()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction = TestDataFactory.CreateTestTransaction(account.Id);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act - page 0 should default to 1
        var result = await controller.GetTransactions(page: 0, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransactionDto>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetTransactions_ShouldHandleLargePageSize()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Act - pageSize > 100 should default to 50
        var result = await controller.GetTransactions(page: 1, pageSize: 200);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransactionDto>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetTransactions_ShouldHandleNegativePageSize()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<TransactionsController>();
        var controller = new TransactionsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Act - negative pageSize should default to 50
        var result = await controller.GetTransactions(page: 1, pageSize: -10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransactionDto>>>(okResult.Value);
        Assert.True(response.Success);
    }
}


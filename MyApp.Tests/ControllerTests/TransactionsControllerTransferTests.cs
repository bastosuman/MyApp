using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class TransactionsControllerTransferTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task CreateTransaction_ShouldHandleTransferType()
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
            TransactionType = "Transfer",
            Amount = 300m,
            Description = "Test transfer"
        };

        // Act
        var result = await controller.CreateTransaction(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionDto>>(createdResult.Value);
        Assert.True(response.Success);

        // Verify account balance was not changed (transfer doesn't modify balance in this implementation)
        var updatedAccount = await context.Accounts.FindAsync(account.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(1000m, updatedAccount.Balance); // Balance unchanged for transfer
    }
}


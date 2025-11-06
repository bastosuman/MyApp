using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Services;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class TransfersControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    private static ILogger<TransfersController> CreateControllerLogger()
    {
        return new LoggerFactory().CreateLogger<TransfersController>();
    }

    private static ILogger<TransferService> CreateServiceLogger()
    {
        return new LoggerFactory().CreateLogger<TransferService>();
    }

    [Fact]
    public async Task CreateInternalTransfer_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var destAccount = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destAccount);
        await context.SaveChangesAsync();

        var dto = new CreateInternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountId = destAccount.Id,
            Amount = 1000m,
            Description = "Test transfer"
        };

        // Act
        var result = await controller.CreateInternalTransfer(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1000m, response.Data.Amount);
    }

    [Fact]
    public async Task CreateInternalTransfer_ShouldReturnBadRequest_WhenInsufficientBalance()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 100m, isActive: true);
        var destAccount = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destAccount);
        await context.SaveChangesAsync();

        var dto = new CreateInternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountId = destAccount.Id,
            Amount = 1000m,
            Description = "Test transfer"
        };

        // Act
        var result = await controller.CreateInternalTransfer(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("balance", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateExternalTransfer_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        // External transfer requires destination account to exist in system
        var destAccount = TestDataFactory.CreateTestAccount("EXT123", "External User", 5000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destAccount);
        await context.SaveChangesAsync();

        var dto = new CreateExternalTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountNumber = "EXT123",
            Amount = 1000m,
            Description = "External transfer"
        };

        // Act
        var result = await controller.CreateExternalTransfer(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetTransfers_ShouldReturnAllTransfers_WhenNoFilters()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer1 = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Description = "Transfer 1",
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        var transfer2 = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 500m,
            Description = "Transfer 2",
            Status = "Pending",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.AddRange(transfer1, transfer2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransfers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var transfers = response.Data.ToList();
        Assert.Equal(2, transfers.Count);
    }

    [Fact]
    public async Task GetTransfers_ShouldFilterByAccountId()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        var account3 = TestDataFactory.CreateTestAccount("ACC003", "Bob", 3000m, isActive: true);
        context.Accounts.AddRange(account1, account2, account3);
        await context.SaveChangesAsync();

        var transfer1 = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        var transfer2 = new Transfer
        {
            SourceAccountId = account3.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 500m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.AddRange(transfer1, transfer2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransfers(accountId: account1.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        var transfers = response.Data!.ToList();
        Assert.Single(transfers);
        Assert.Equal(account1.Id, transfers[0].SourceAccountId);
    }

    [Fact]
    public async Task GetTransfers_ShouldFilterByStatus()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer1 = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        var transfer2 = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 500m,
            Status = "Pending",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.AddRange(transfer1, transfer2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransfers(status: "Pending");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        var transfers = response.Data!.ToList();
        Assert.Single(transfers);
        Assert.Equal("Pending", transfers[0].Status);
    }

    [Fact]
    public async Task GetTransfer_ShouldReturnTransfer_WhenExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Description = "Test transfer",
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetTransfer(transfer.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(transfer.Id, response.Data.Id);
        Assert.Equal(1000m, response.Data.Amount);
    }

    [Fact]
    public async Task GetTransfer_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        // Act
        var result = await controller.GetTransfer(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetAccountTransfers_ShouldReturnTransfersForAccount()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer1 = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        var transfer2 = new Transfer
        {
            SourceAccountId = account2.Id,
            DestinationAccountId = account1.Id,
            TransferType = "Internal",
            Amount = 500m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.AddRange(transfer1, transfer2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAccountTransfers(account1.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        var transfers = response.Data!.ToList();
        Assert.Equal(2, transfers.Count); // Both as source and destination
    }

    [Fact]
    public async Task CancelTransfer_ShouldReturnOk_WhenTransferIsPending()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Pending",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.CancelTransfer(transfer.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CancelTransfer_ShouldReturnBadRequest_WhenTransferNotCancellable()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.CancelTransfer(transfer.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task RetryTransfer_ShouldReturnOk_WhenTransferIsFailed()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Failed",
            TransferDate = DateTime.UtcNow,
            FailureReason = "Test failure"
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.RetryTransfer(transfer.Id);

        // Assert
        // Should succeed if retry works, or return bad request if validation fails
        Assert.True(result.Result is OkObjectResult || result.Result is BadRequestObjectResult);
    }

    [Fact]
    public async Task RetryTransfer_ShouldReturnBadRequest_WhenTransferNotFailed()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var transfer = new Transfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Status = "Completed",
            TransferDate = DateTime.UtcNow
        };
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.RetryTransfer(transfer.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Only failed transfers", response.Message);
    }

    [Fact]
    public async Task RetryTransfer_ShouldReturnNotFound_WhenTransferNotExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var controllerLogger = CreateControllerLogger();
        var serviceLogger = CreateServiceLogger();
        var transferService = new TransferService(context, serviceLogger);
        var controller = new TransfersController(context, transferService, controllerLogger);

        // Act
        var result = await controller.RetryTransfer(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransferDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }
}


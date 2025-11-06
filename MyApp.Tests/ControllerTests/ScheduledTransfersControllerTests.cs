using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class ScheduledTransfersControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    private static ILogger<ScheduledTransfersController> CreateLogger()
    {
        return new LoggerFactory().CreateLogger<ScheduledTransfersController>();
    }

    [Fact]
    public async Task CreateScheduledTransfer_ShouldReturnCreated_WhenValidInternal()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var destAccount = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destAccount);
        await context.SaveChangesAsync();

        var dto = new CreateScheduledTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountId = destAccount.Id,
            Amount = 1000m,
            Description = "Scheduled transfer",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime"
        };

        // Act
        var result = await controller.CreateScheduledTransfer(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1000m, response.Data.Amount);
    }

    [Fact]
    public async Task CreateScheduledTransfer_ShouldReturnCreated_WhenValidExternal()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var destAccount = TestDataFactory.CreateTestAccount("EXT123", "External", 5000m, isActive: true);
        context.Accounts.AddRange(sourceAccount, destAccount);
        await context.SaveChangesAsync();

        var dto = new CreateScheduledTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountNumber = "EXT123",
            Amount = 1000m,
            Description = "External scheduled transfer",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "Monthly",
            RecurrenceDay = 15
        };

        // Act
        var result = await controller.CreateScheduledTransfer(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("External", response.Data.TransferType);
    }

    [Fact]
    public async Task CreateScheduledTransfer_ShouldReturnBadRequest_WhenSourceAccountNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var dto = new CreateScheduledTransferDto
        {
            SourceAccountId = 999,
            DestinationAccountId = 1,
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime"
        };

        // Act
        var result = await controller.CreateScheduledTransfer(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Source account", response.Message);
    }

    [Fact]
    public async Task CreateScheduledTransfer_ShouldReturnBadRequest_WhenDestinationAccountNotFound()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var sourceAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        context.Accounts.Add(sourceAccount);
        await context.SaveChangesAsync();

        var dto = new CreateScheduledTransferDto
        {
            SourceAccountId = sourceAccount.Id,
            DestinationAccountId = 999,
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime"
        };

        // Act
        var result = await controller.CreateScheduledTransfer(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Destination account", response.Message);
    }

    [Fact]
    public async Task GetScheduledTransfers_ShouldReturnAll_WhenNoFilters()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled1 = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        var scheduled2 = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 500m,
            ScheduledDate = DateTime.UtcNow.AddDays(2),
            RecurrenceType = "Monthly",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.AddRange(scheduled1, scheduled2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetScheduledTransfers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ScheduledTransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var transfers = response.Data.ToList();
        Assert.Equal(2, transfers.Count);
    }

    [Fact]
    public async Task GetScheduledTransfers_ShouldFilterByAccountId()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        var account3 = TestDataFactory.CreateTestAccount("ACC003", "Bob", 3000m, isActive: true);
        context.Accounts.AddRange(account1, account2, account3);
        await context.SaveChangesAsync();

        var scheduled1 = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        var scheduled2 = new ScheduledTransfer
        {
            SourceAccountId = account3.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 500m,
            ScheduledDate = DateTime.UtcNow.AddDays(2),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.AddRange(scheduled1, scheduled2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetScheduledTransfers(accountId: account1.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ScheduledTransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        var transfers = response.Data!.ToList();
        Assert.Single(transfers);
        Assert.Equal(account1.Id, transfers[0].SourceAccountId);
    }

    [Fact]
    public async Task GetScheduledTransfers_ShouldFilterByStatus()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled1 = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        var scheduled2 = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 500m,
            ScheduledDate = DateTime.UtcNow.AddDays(2),
            RecurrenceType = "OneTime",
            Status = "Paused",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.AddRange(scheduled1, scheduled2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetScheduledTransfers(status: "Paused");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ScheduledTransferDto>>>(okResult.Value);
        Assert.True(response.Success);
        var transfers = response.Data!.ToList();
        Assert.Single(transfers);
        Assert.Equal("Paused", transfers[0].Status);
    }

    [Fact]
    public async Task GetScheduledTransfer_ShouldReturnTransfer_WhenExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            Description = "Test scheduled transfer",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetScheduledTransfer(scheduled.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(scheduled.Id, response.Data.Id);
        Assert.Equal(1000m, response.Data.Amount);
    }

    [Fact]
    public async Task GetScheduledTransfer_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        // Act
        var result = await controller.GetScheduledTransfer(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateScheduledTransfer_ShouldReturnOk_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        var updateDto = new UpdateScheduledTransferDto
        {
            Amount = 1500m,
            Description = "Updated description"
        };

        // Act
        var result = await controller.UpdateScheduledTransfer(scheduled.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1500m, response.Data.Amount);
    }

    [Fact]
    public async Task UpdateScheduledTransfer_ShouldReturnBadRequest_WhenNotActiveOrPaused()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Cancelled",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        var updateDto = new UpdateScheduledTransferDto
        {
            Amount = 1500m
        };

        // Act
        var result = await controller.UpdateScheduledTransfer(scheduled.Id, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ScheduledTransferDto>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task CancelScheduledTransfer_ShouldReturnOk_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.CancelScheduledTransfer(scheduled.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);

        // Verify status changed
        await context.Entry(scheduled).ReloadAsync();
        Assert.Equal("Cancelled", scheduled.Status);
    }

    [Fact]
    public async Task CancelScheduledTransfer_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        // Act
        var result = await controller.CancelScheduledTransfer(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task PauseScheduledTransfer_ShouldReturnOk_WhenActive()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.PauseScheduledTransfer(scheduled.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);

        // Verify status changed
        await context.Entry(scheduled).ReloadAsync();
        Assert.Equal("Paused", scheduled.Status);
    }

    [Fact]
    public async Task PauseScheduledTransfer_ShouldReturnBadRequest_WhenNotActive()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Paused",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.PauseScheduledTransfer(scheduled.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task ResumeScheduledTransfer_ShouldReturnOk_WhenPaused()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Paused",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ResumeScheduledTransfer(scheduled.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);

        // Verify status changed
        await context.Entry(scheduled).ReloadAsync();
        Assert.Equal("Active", scheduled.Status);
    }

    [Fact]
    public async Task ResumeScheduledTransfer_ShouldReturnBadRequest_WhenNotPaused()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = CreateLogger();
        var controller = new ScheduledTransfersController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 10000m, isActive: true);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 5000m, isActive: true);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var scheduled = new ScheduledTransfer
        {
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            TransferType = "Internal",
            Amount = 1000m,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            RecurrenceType = "OneTime",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        context.ScheduledTransfers.Add(scheduled);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ResumeScheduledTransfer(scheduled.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
    }
}


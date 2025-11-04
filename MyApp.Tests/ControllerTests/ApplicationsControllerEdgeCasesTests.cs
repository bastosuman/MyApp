using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class ApplicationsControllerEdgeCasesTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnBadRequest_WhenAmountIsAtMinBoundary()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct(minAmount: 1000m, maxAmount: 10000m, isActive: true);
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createDto = new CreateApplicationDto
        {
            AccountId = account.Id,
            ProductId = product.Id,
            RequestedAmount = 999.99m, // Just below minimum
            Notes = "Test"
        };

        // Act
        var result = await controller.CreateApplication(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("must be between", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnBadRequest_WhenAmountIsAtMaxBoundary()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct(minAmount: 1000m, maxAmount: 10000m, isActive: true);
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createDto = new CreateApplicationDto
        {
            AccountId = account.Id,
            ProductId = product.Id,
            RequestedAmount = 10000.01m, // Just above maximum
            Notes = "Test"
        };

        // Act
        var result = await controller.CreateApplication(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("must be between", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateApplication_ShouldAcceptAmount_AtMinBoundary()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct(minAmount: 1000m, maxAmount: 10000m, isActive: true);
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createDto = new CreateApplicationDto
        {
            AccountId = account.Id,
            ProductId = product.Id,
            RequestedAmount = 1000m, // Exactly at minimum (should be valid)
            Notes = "Test"
        };

        // Act
        var result = await controller.CreateApplication(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(createdResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateApplication_ShouldAcceptAmount_AtMaxBoundary()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct(minAmount: 1000m, maxAmount: 10000m, isActive: true);
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createDto = new CreateApplicationDto
        {
            AccountId = account.Id,
            ProductId = product.Id,
            RequestedAmount = 10000m, // Exactly at maximum (should be valid)
            Notes = "Test"
        };

        // Act
        var result = await controller.CreateApplication(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(createdResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateApplicationStatus_ShouldNotUpdateNotes_WhenNotesIsEmpty()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct();
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var application = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m, "Pending");
        application.Notes = "Original notes";
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        var updateDto = new UpdateApplicationStatusDto
        {
            Status = "Approved",
            Notes = "" // Empty string
        };

        // Act
        var result = await controller.UpdateApplicationStatus(application.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(okResult.Value);
        Assert.True(response.Success);
        
        // Verify original notes were preserved
        var updatedApplication = await context.Applications.FindAsync(application.Id);
        Assert.NotNull(updatedApplication);
        Assert.Equal("Original notes", updatedApplication.Notes);
    }

    [Fact]
    public async Task UpdateApplicationStatus_ShouldNotUpdateNotes_WhenNotesIsNull()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct();
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var application = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m, "Pending");
        application.Notes = "Original notes";
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        var updateDto = new UpdateApplicationStatusDto
        {
            Status = "Approved",
            Notes = null // Null
        };

        // Act
        var result = await controller.UpdateApplicationStatus(application.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(okResult.Value);
        Assert.True(response.Success);
        
        // Verify original notes were preserved
        var updatedApplication = await context.Applications.FindAsync(application.Id);
        Assert.NotNull(updatedApplication);
        Assert.Equal("Original notes", updatedApplication.Notes);
    }
}


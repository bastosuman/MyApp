using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class ApplicationsControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task GetApplications_ShouldReturnAllApplications_WhenNoFilter()
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

        var application1 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m, "Pending");
        var application2 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 10000m, "Approved");
        context.Applications.AddRange(application1, application2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetApplications();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ApplicationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var applications = response.Data.ToList();
        Assert.Equal(2, applications.Count);
    }

    [Fact]
    public async Task GetApplications_ShouldFilterByStatus_WhenStatusProvided()
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

        var application1 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m, "Pending");
        var application2 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 10000m, "Approved");
        context.Applications.AddRange(application1, application2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetApplications(status: "Pending");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ApplicationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var applications = response.Data.ToList();
        Assert.Single(applications);
        Assert.Equal("Pending", applications[0].Status);
    }

    [Fact]
    public async Task GetApplication_ShouldReturnApplication_WhenExists()
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

        var application = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m);
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetApplication(application.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(application.RequestedAmount, response.Data.RequestedAmount);
        Assert.Equal(account.AccountNumber, response.Data.AccountNumber);
        Assert.Equal(product.Name, response.Data.ProductName);
    }

    [Fact]
    public async Task CreateApplication_ShouldCreateApplication_WhenValid()
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
            RequestedAmount = 5000m,
            Notes = "Test application"
        };

        // Act
        var result = await controller.CreateApplication(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Pending", response.Data.Status);
        Assert.Equal(createDto.RequestedAmount, response.Data.RequestedAmount);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnBadRequest_WhenAmountOutsideProductLimits()
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
            RequestedAmount = 15000m, // Outside limits
            Notes = "Test application"
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
    public async Task CreateApplication_ShouldReturnBadRequest_WhenProductInactive()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct(isActive: false);
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createDto = new CreateApplicationDto
        {
            AccountId = account.Id,
            ProductId = product.Id,
            RequestedAmount = 5000m
        };

        // Act
        var result = await controller.CreateApplication(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("not active", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateApplicationStatus_ShouldUpdateStatus_WhenValid()
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
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        var updateDto = new UpdateApplicationStatusDto
        {
            Status = "Approved",
            Notes = "Application approved"
        };

        // Act
        var result = await controller.UpdateApplicationStatus(application.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Approved", response.Data!.Status);
        Assert.NotNull(response.Data.DecisionDate);
    }

    [Fact]
    public async Task UpdateApplicationStatus_ShouldReturnBadRequest_WhenInvalidStatus()
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

        var application = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m);
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        var updateDto = new UpdateApplicationStatusDto
        {
            Status = "InvalidStatus"
        };

        // Act
        var result = await controller.UpdateApplicationStatus(application.Id, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ApplicationDto>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetApplicationsByAccount_ShouldReturnApplications_ForAccount()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ApplicationsController>();
        var controller = new ApplicationsController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001");
        var account2 = TestDataFactory.CreateTestAccount("ACC002");
        var product = TestDataFactory.CreateTestProduct();
        context.Accounts.AddRange(account1, account2);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var application1 = TestDataFactory.CreateTestApplication(account1.Id, product.Id, 5000m);
        var application2 = TestDataFactory.CreateTestApplication(account1.Id, product.Id, 10000m);
        var application3 = TestDataFactory.CreateTestApplication(account2.Id, product.Id, 15000m);
        context.Applications.AddRange(application1, application2, application3);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetApplicationsByAccount(account1.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ApplicationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var applications = response.Data.ToList();
        Assert.Equal(2, applications.Count);
        Assert.All(applications, a => Assert.Equal(account1.Id, a.AccountId));
    }
}


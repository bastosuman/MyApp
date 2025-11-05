using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class DashboardControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnDashboardData_WithAllSections()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        // Create test data
        var activeAccount1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 5000m, isActive: true);
        var activeAccount2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 3000m, isActive: true);
        var inactiveAccount = TestDataFactory.CreateTestAccount("ACC003", "Bob Johnson", 2000m, isActive: false);
        context.Accounts.AddRange(activeAccount1, activeAccount2, inactiveAccount);
        await context.SaveChangesAsync();

        // Create transactions
        var transaction1 = TestDataFactory.CreateTestTransaction(activeAccount1.Id, "Deposit", 1000m);
        var transaction2 = TestDataFactory.CreateTestTransaction(activeAccount1.Id, "Withdrawal", 500m);
        var transaction3 = TestDataFactory.CreateTestTransaction(activeAccount2.Id, "Deposit", 2000m);
        context.Transactions.AddRange(transaction1, transaction2, transaction3);
        await context.SaveChangesAsync();

        // Create applications
        var product = TestDataFactory.CreateTestProduct("Personal Loan", isActive: true);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var app1 = TestDataFactory.CreateTestApplication(activeAccount1.Id, product.Id, 5000m, "Pending");
        var app2 = TestDataFactory.CreateTestApplication(activeAccount2.Id, product.Id, 10000m, "Approved");
        var app3 = TestDataFactory.CreateTestApplication(activeAccount1.Id, product.Id, 7500m, "Rejected");
        context.Applications.AddRange(app1, app2, app3);
        await context.SaveChangesAsync();

        // Create products
        var activeProduct2 = TestDataFactory.CreateTestProduct("Home Loan", isActive: true);
        var inactiveProduct = TestDataFactory.CreateTestProduct("Old Product", isActive: false);
        context.Products.AddRange(activeProduct2, inactiveProduct);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var dashboard = response.Data;
        
        // Verify account summary (only active accounts)
        Assert.NotNull(dashboard.AccountSummary);
        Assert.Equal(8000m, dashboard.AccountSummary.TotalBalance); // 5000 + 3000
        Assert.Equal(2, dashboard.AccountSummary.AccountCount);
        Assert.Equal(2, dashboard.AccountSummary.Accounts.Count);
        
        // Verify recent transactions (should return all 3, ordered by date descending)
        Assert.NotNull(dashboard.RecentTransactions);
        Assert.Equal(3, dashboard.RecentTransactions.Count);
        
        // Verify application status
        Assert.NotNull(dashboard.ApplicationStatus);
        Assert.Equal(1, dashboard.ApplicationStatus.Pending);
        Assert.Equal(1, dashboard.ApplicationStatus.Approved);
        Assert.Equal(1, dashboard.ApplicationStatus.Rejected);
        Assert.Equal(3, dashboard.ApplicationStatus.Total);
        
        // Verify products count (only active) - product created earlier + activeProduct2
        Assert.Equal(2, dashboard.AvailableProductsCount);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnOnlyLast10Transactions()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Create 15 transactions
        for (int i = 0; i < 15; i++)
        {
            var transaction = TestDataFactory.CreateTestTransaction(
                account.Id, 
                "Deposit", 
                100m + i,
                $"Transaction {i}");
            transaction.TransactionDate = DateTime.UtcNow.AddMinutes(-i);
            context.Transactions.Add(transaction);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(10, response.Data.RecentTransactions.Count);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnEmptyData_WhenNoDataExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var dashboard = response.Data;
        
        // Verify all sections are empty but initialized
        Assert.NotNull(dashboard.AccountSummary);
        Assert.Equal(0m, dashboard.AccountSummary.TotalBalance);
        Assert.Equal(0, dashboard.AccountSummary.AccountCount);
        Assert.Empty(dashboard.AccountSummary.Accounts);
        
        Assert.NotNull(dashboard.RecentTransactions);
        Assert.Empty(dashboard.RecentTransactions);
        
        Assert.NotNull(dashboard.ApplicationStatus);
        Assert.Equal(0, dashboard.ApplicationStatus.Pending);
        Assert.Equal(0, dashboard.ApplicationStatus.Approved);
        Assert.Equal(0, dashboard.ApplicationStatus.Rejected);
        Assert.Equal(0, dashboard.ApplicationStatus.Total);
        
        Assert.Equal(0, dashboard.AvailableProductsCount);
    }

    [Fact]
    public async Task GetDashboard_ShouldExcludeInactiveAccounts()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        var activeAccount = TestDataFactory.CreateTestAccount("ACC001", "Active User", 5000m, isActive: true);
        var inactiveAccount = TestDataFactory.CreateTestAccount("ACC002", "Inactive User", 3000m, isActive: false);
        context.Accounts.AddRange(activeAccount, inactiveAccount);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var dashboard = response.Data;
        Assert.Equal(5000m, dashboard.AccountSummary.TotalBalance); // Only active account
        Assert.Equal(1, dashboard.AccountSummary.AccountCount);
        Assert.Single(dashboard.AccountSummary.Accounts);
        Assert.Equal("ACC001", dashboard.AccountSummary.Accounts[0].AccountNumber);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnTransactionsOrderedByDateDescending()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction1 = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 100m);
        transaction1.TransactionDate = DateTime.UtcNow.AddDays(-3);
        
        var transaction2 = TestDataFactory.CreateTestTransaction(account.Id, "Withdrawal", 200m);
        transaction2.TransactionDate = DateTime.UtcNow.AddDays(-1);
        
        var transaction3 = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 300m);
        transaction3.TransactionDate = DateTime.UtcNow.AddDays(-2);

        context.Transactions.AddRange(transaction1, transaction2, transaction3);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var transactions = response.Data.RecentTransactions.ToList();
        Assert.Equal(3, transactions.Count);
        // Should be ordered by date descending (most recent first)
        Assert.True(transactions[0].TransactionDate >= transactions[1].TransactionDate);
        Assert.True(transactions[1].TransactionDate >= transactions[2].TransactionDate);
    }

    [Fact]
    public async Task GetDashboard_ShouldIncludeAccountNumberInTransactions()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 500m);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var transactions = response.Data.RecentTransactions.ToList();
        Assert.Single(transactions);
        Assert.Equal("ACC001", transactions[0].AccountNumber);
    }

    [Fact]
    public async Task GetDashboard_ShouldCalculateApplicationStatusCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        var account = TestDataFactory.CreateTestAccount();
        var product = TestDataFactory.CreateTestProduct();
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Create applications with different statuses
        var pending1 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 1000m, "Pending");
        var pending2 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 2000m, "Pending");
        var approved1 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 3000m, "Approved");
        var approved2 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 4000m, "Approved");
        var approved3 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m, "Approved");
        var rejected1 = TestDataFactory.CreateTestApplication(account.Id, product.Id, 6000m, "Rejected");
        
        context.Applications.AddRange(pending1, pending2, approved1, approved2, approved3, rejected1);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var status = response.Data.ApplicationStatus;
        Assert.Equal(2, status.Pending);
        Assert.Equal(3, status.Approved);
        Assert.Equal(1, status.Rejected);
        Assert.Equal(6, status.Total);
    }

    [Fact]
    public async Task GetDashboard_ShouldCountOnlyActiveProducts()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        var activeProduct1 = TestDataFactory.CreateTestProduct("Product 1", isActive: true);
        var activeProduct2 = TestDataFactory.CreateTestProduct("Product 2", isActive: true);
        var inactiveProduct1 = TestDataFactory.CreateTestProduct("Product 3", isActive: false);
        var inactiveProduct2 = TestDataFactory.CreateTestProduct("Product 4", isActive: false);
        
        context.Products.AddRange(activeProduct1, activeProduct2, inactiveProduct1, inactiveProduct2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.AvailableProductsCount);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnTransactionsWithAccountNumbers()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<DashboardController>();
        var controller = new DashboardController(context, logger);

        // Create an account and transaction
        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 100m);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DashboardDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var transactions = response.Data.RecentTransactions.ToList();
        Assert.Single(transactions);
        // The account number should be populated from the included account
        Assert.Equal("ACC001", transactions[0].AccountNumber);
    }
}


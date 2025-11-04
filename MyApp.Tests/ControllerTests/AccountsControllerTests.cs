using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class AccountsControllerTests
{
    private FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task GetAccounts_ShouldReturnAllAccounts()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var account1 = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        var account2 = TestDataFactory.CreateTestAccount("ACC002", "Jane Smith", 2000m);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAccounts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<AccountDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var accounts = response.Data.ToList();
        Assert.Equal(2, accounts.Count);
    }

    [Fact]
    public async Task GetAccount_ShouldReturnAccount_WhenExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAccount(account.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(account.AccountNumber, response.Data.AccountNumber);
        Assert.Equal(account.AccountHolderName, response.Data.AccountHolderName);
    }

    [Fact]
    public async Task GetAccount_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        // Act
        var result = await controller.GetAccount(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task CreateAccount_ShouldCreateAccount_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var createDto = new CreateAccountDto
        {
            AccountNumber = "ACC003",
            AccountHolderName = "Bob Johnson",
            Balance = 3000m,
            AccountType = "Savings"
        };

        // Act
        var result = await controller.CreateAccount(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(createDto.AccountNumber, response.Data.AccountNumber);

        // Verify in database
        var savedAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == "ACC003");
        Assert.NotNull(savedAccount);
        Assert.Equal(createDto.AccountHolderName, savedAccount!.AccountHolderName);
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnBadRequest_WhenAccountNumberExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var existingAccount = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        context.Accounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var createDto = new CreateAccountDto
        {
            AccountNumber = "ACC001", // Duplicate
            AccountHolderName = "Jane Smith",
            Balance = 2000m,
            AccountType = "Checking"
        };

        // Act
        var result = await controller.CreateAccount(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("already exists", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAccount_ShouldUpdateAccount_WhenValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var updateDto = new UpdateAccountDto
        {
            AccountHolderName = "John Updated",
            Balance = 5000m,
            AccountType = "Checking",
            IsActive = true
        };

        // Act
        var result = await controller.UpdateAccount(account.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(updateDto.AccountHolderName, response.Data!.AccountHolderName);
        Assert.Equal(updateDto.Balance, response.Data.Balance);
    }

    [Fact]
    public async Task UpdateAccount_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var updateDto = new UpdateAccountDto
        {
            AccountHolderName = "John Doe",
            Balance = 1000m,
            AccountType = "Savings",
            IsActive = true
        };

        // Act
        var result = await controller.UpdateAccount(999, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetAccountTransactions_ShouldReturnTransactions_WhenAccountExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction1 = TestDataFactory.CreateTestTransaction(account.Id, "Deposit", 500m);
        var transaction2 = TestDataFactory.CreateTestTransaction(account.Id, "Withdrawal", 200m);
        context.Transactions.AddRange(transaction1, transaction2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAccountTransactions(account.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TransactionDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var transactions = response.Data.ToList();
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public async Task GetAccountApplications_ShouldReturnApplications_WhenAccountExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AccountsController>();
        var controller = new AccountsController(context, logger);

        var account = TestDataFactory.CreateTestAccount("ACC001", "John Doe", 1000m);
        var product = TestDataFactory.CreateTestProduct();
        context.Accounts.Add(account);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var application = TestDataFactory.CreateTestApplication(account.Id, product.Id, 5000m);
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAccountApplications(account.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ApplicationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var applications = response.Data.ToList();
        Assert.Single(applications);
    }
}


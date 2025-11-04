using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Tests.DataTests;

public class DbInitializerTests
{
    private FinancialDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinancialDbContext(options);
    }

    [Fact]
    public void DbInitializer_ShouldCreateDatabase()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        Assert.True(context.Database.CanConnect());
    }

    [Fact]
    public void DbInitializer_ShouldSeedProducts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var products = context.Products.ToList();
        Assert.Equal(4, products.Count);
        Assert.Contains(products, p => p.Name == "Personal Loan");
        Assert.Contains(products, p => p.Name == "Home Loan");
        Assert.Contains(products, p => p.Name == "Credit Card");
        Assert.Contains(products, p => p.Name == "Savings Account");
    }

    [Fact]
    public void DbInitializer_ShouldSeedProductsWithCorrectData()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var personalLoan = context.Products.First(p => p.Name == "Personal Loan");
        Assert.Equal("Loan", personalLoan.ProductType);
        Assert.Equal(5.5m, personalLoan.InterestRate);
        Assert.Equal(1000m, personalLoan.MinAmount);
        Assert.Equal(50000m, personalLoan.MaxAmount);
        Assert.True(personalLoan.IsActive);

        var homeLoan = context.Products.First(p => p.Name == "Home Loan");
        Assert.Equal("Loan", homeLoan.ProductType);
        Assert.Equal(4.2m, homeLoan.InterestRate);
        Assert.Equal(50000m, homeLoan.MinAmount);
        Assert.Equal(500000m, homeLoan.MaxAmount);

        var creditCard = context.Products.First(p => p.Name == "Credit Card");
        Assert.Equal("CreditCard", creditCard.ProductType);
        Assert.Equal(18.9m, creditCard.InterestRate);
        Assert.Equal(500m, creditCard.MinAmount);
        Assert.Equal(10000m, creditCard.MaxAmount);

        var savingsAccount = context.Products.First(p => p.Name == "Savings Account");
        Assert.Equal("SavingsAccount", savingsAccount.ProductType);
        Assert.Equal(2.5m, savingsAccount.InterestRate);
        Assert.Equal(0m, savingsAccount.MinAmount);
        Assert.Equal(1000000m, savingsAccount.MaxAmount);
    }

    [Fact]
    public void DbInitializer_ShouldSeedAccounts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var accounts = context.Accounts.ToList();
        Assert.Equal(3, accounts.Count);
        Assert.Contains(accounts, a => a.AccountNumber == "ACC001");
        Assert.Contains(accounts, a => a.AccountNumber == "ACC002");
        Assert.Contains(accounts, a => a.AccountNumber == "ACC003");
    }

    [Fact]
    public void DbInitializer_ShouldSeedAccountsWithCorrectData()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var account1 = context.Accounts.First(a => a.AccountNumber == "ACC001");
        Assert.Equal("John Doe", account1.AccountHolderName);
        Assert.Equal(15000.00m, account1.Balance);
        Assert.Equal("Savings", account1.AccountType);
        Assert.True(account1.IsActive);

        var account2 = context.Accounts.First(a => a.AccountNumber == "ACC002");
        Assert.Equal("Jane Smith", account2.AccountHolderName);
        Assert.Equal(8500.50m, account2.Balance);
        Assert.Equal("Checking", account2.AccountType);

        var account3 = context.Accounts.First(a => a.AccountNumber == "ACC003");
        Assert.Equal("Bob Johnson", account3.AccountHolderName);
        Assert.Equal(25000.00m, account3.Balance);
        Assert.Equal("Savings", account3.AccountType);
    }

    [Fact]
    public void DbInitializer_ShouldSeedTransactions()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var transactions = context.Transactions.ToList();
        Assert.Equal(4, transactions.Count);
    }

    [Fact]
    public void DbInitializer_ShouldNotSeedIfProductsAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        
        // Seed once
        DbInitializer.Initialize(context);
        var initialProductCount = context.Products.Count();

        // Add a product manually
        context.Products.Add(new Product
        {
            Name = "Test Product",
            ProductType = "Loan",
            InterestRate = 10m,
            MinAmount = 100m,
            MaxAmount = 1000m,
            Description = "Test",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        });
        context.SaveChanges();
        var productCountAfterManualAdd = context.Products.Count();

        // Act - Initialize again
        DbInitializer.Initialize(context);

        // Assert - Should not seed again because products already exist
        Assert.Equal(productCountAfterManualAdd, context.Products.Count());
        Assert.True(context.Products.Any(p => p.Name == "Test Product"));
    }

    [Fact]
    public void DbInitializer_ShouldSetCreatedDates()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var beforeInit = DateTime.UtcNow;

        // Act
        DbInitializer.Initialize(context);
        var afterInit = DateTime.UtcNow;

        // Assert
        var products = context.Products.ToList();
        foreach (var product in products)
        {
            Assert.True(product.CreatedDate >= beforeInit.AddMinutes(-1));
            Assert.True(product.CreatedDate <= afterInit.AddMinutes(1));
        }

        var accounts = context.Accounts.ToList();
        foreach (var account in accounts)
        {
            Assert.True(account.CreatedDate >= beforeInit.AddMonths(-12).AddMinutes(-1));
            Assert.True(account.CreatedDate <= afterInit.AddMinutes(1));
        }
    }
}


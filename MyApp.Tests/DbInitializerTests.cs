using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Data;
using Xunit;

namespace MyApp.Tests;

public class DbInitializerTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Seed_EmptyDatabase_AddsProducts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        Assert.Equal(4, products.Count);
        Assert.Contains(products, p => p.Name == "Personal Loan");
        Assert.Contains(products, p => p.Name == "Home Loan");
        Assert.Contains(products, p => p.Name == "Credit Card");
        Assert.Contains(products, p => p.Name == "Savings Account");
    }

    [Fact]
    public void Seed_EmptyDatabase_AddsAccounts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var accounts = context.Accounts.ToList();
        Assert.Equal(3, accounts.Count);
        Assert.Contains(accounts, a => a.AccountNumber == "ACC001");
        Assert.Contains(accounts, a => a.AccountNumber == "ACC002");
        Assert.Contains(accounts, a => a.AccountNumber == "ACC003");
    }

    [Fact]
    public void Seed_EmptyDatabase_ProductsHaveCorrectProperties()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var personalLoan = context.Products.First(p => p.Name == "Personal Loan");
        Assert.Equal("Loan", personalLoan.ProductType);
        Assert.Equal(1000.00m, personalLoan.MinAmount);
        Assert.Equal(50000.00m, personalLoan.MaxAmount);
        Assert.Equal(5.5m, personalLoan.InterestRate);
        Assert.Equal(12, personalLoan.MinTermMonths);
        Assert.Equal(60, personalLoan.MaxTermMonths);
        Assert.True(personalLoan.IsActive);
        Assert.Contains("Flexible personal loans", personalLoan.Description);

        var homeLoan = context.Products.First(p => p.Name == "Home Loan");
        Assert.Equal("Mortgage", homeLoan.ProductType);
        Assert.Equal(50000.00m, homeLoan.MinAmount);
        Assert.Equal(500000.00m, homeLoan.MaxAmount);
        Assert.Equal(4.2m, homeLoan.InterestRate);
        Assert.Equal(60, homeLoan.MinTermMonths);
        Assert.Equal(360, homeLoan.MaxTermMonths);
        Assert.True(homeLoan.IsActive);

        var creditCard = context.Products.First(p => p.Name == "Credit Card");
        Assert.Equal("Credit", creditCard.ProductType);
        Assert.Equal(500.00m, creditCard.MinAmount);
        Assert.Equal(10000.00m, creditCard.MaxAmount);
        Assert.Equal(18.9m, creditCard.InterestRate);
        Assert.True(creditCard.IsActive);

        var savingsAccount = context.Products.First(p => p.Name == "Savings Account");
        Assert.Equal("Deposit", savingsAccount.ProductType);
        Assert.Equal(0.00m, savingsAccount.MinAmount);
        Assert.Equal(1000000.00m, savingsAccount.MaxAmount);
        Assert.Equal(2.5m, savingsAccount.InterestRate);
        Assert.Equal(0, savingsAccount.MinTermMonths);
        Assert.Equal(0, savingsAccount.MaxTermMonths);
        Assert.True(savingsAccount.IsActive);
    }

    [Fact]
    public void Seed_EmptyDatabase_AccountsHaveCorrectProperties()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var account1 = context.Accounts.First(a => a.AccountNumber == "ACC001");
        Assert.Equal("John", account1.FirstName);
        Assert.Equal("Doe", account1.LastName);
        Assert.Equal("john.doe@example.com", account1.Email);
        Assert.Equal("+1-555-0101", account1.Phone);
        Assert.True(account1.IsActive);
        Assert.True(account1.DateCreated <= DateTime.UtcNow);
        Assert.True(account1.DateCreated >= DateTime.UtcNow.AddDays(-31));

        var account2 = context.Accounts.First(a => a.AccountNumber == "ACC002");
        Assert.Equal("Jane", account2.FirstName);
        Assert.Equal("Smith", account2.LastName);
        Assert.Equal("jane.smith@example.com", account2.Email);
        Assert.Equal("+1-555-0102", account2.Phone);
        Assert.True(account2.IsActive);

        var account3 = context.Accounts.First(a => a.AccountNumber == "ACC003");
        Assert.Equal("Robert", account3.FirstName);
        Assert.Equal("Johnson", account3.LastName);
        Assert.Equal("robert.johnson@example.com", account3.Email);
        Assert.Equal("+1-555-0103", account3.Phone);
        Assert.True(account3.IsActive);
    }

    [Fact]
    public void Seed_DatabaseAlreadyHasProducts_DoesNotAddAgain()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Products.Add(new Product
        {
            Name = "Existing Product",
            ProductType = "Loan",
            MinAmount = 1000m,
            MaxAmount = 10000m,
            InterestRate = 5.0m,
            MinTermMonths = 12,
            MaxTermMonths = 60,
            IsActive = true,
            Description = "Existing"
        });
        context.SaveChanges();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        Assert.Equal(1, products.Count);
        Assert.Contains(products, p => p.Name == "Existing Product");
        Assert.DoesNotContain(products, p => p.Name == "Personal Loan");
    }

    [Fact]
    public void Seed_DatabaseAlreadyHasAccounts_DoesNotAddAgain()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Accounts.Add(new Account
        {
            AccountNumber = "EXISTING001",
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@example.com",
            Phone = "+1-555-9999",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        });
        context.SaveChanges();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var accounts = context.Accounts.ToList();
        Assert.Equal(1, accounts.Count);
        Assert.Contains(accounts, a => a.AccountNumber == "EXISTING001");
        Assert.DoesNotContain(accounts, a => a.AccountNumber == "ACC001");
    }

    [Fact]
    public void Seed_DatabaseHasProductsButNoAccounts_DoesNotAddProductsButAddsAccounts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Products.Add(new Product
        {
            Name = "Existing Product",
            ProductType = "Loan",
            MinAmount = 1000m,
            MaxAmount = 10000m,
            InterestRate = 5.0m,
            MinTermMonths = 12,
            MaxTermMonths = 60,
            IsActive = true,
            Description = "Existing"
        });
        context.SaveChanges();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        var accounts = context.Accounts.ToList();
        Assert.Equal(1, products.Count); // Only existing product
        Assert.Equal(3, accounts.Count); // Should add seed accounts
    }

    [Fact]
    public void Seed_DatabaseHasAccountsButNoProducts_DoesNotAddAccountsButAddsProducts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Accounts.Add(new Account
        {
            AccountNumber = "EXISTING001",
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@example.com",
            Phone = "+1-555-9999",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        });
        context.SaveChanges();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        var accounts = context.Accounts.ToList();
        Assert.Equal(4, products.Count); // Should add seed products
        Assert.Equal(1, accounts.Count); // Only existing account
    }

    [Fact]
    public void Seed_MultipleCalls_DoesNotDuplicateData()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);
        DbInitializer.Seed(context);
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        var accounts = context.Accounts.ToList();
        Assert.Equal(4, products.Count);
        Assert.Equal(3, accounts.Count);
    }

    [Fact]
    public void Seed_AllProductsAreActive()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        Assert.All(products, p => Assert.True(p.IsActive));
    }

    [Fact]
    public void Seed_AllAccountsAreActive()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var accounts = context.Accounts.ToList();
        Assert.All(accounts, a => Assert.True(a.IsActive));
    }

    [Fact]
    public void Seed_ProductsHaveUniqueNames()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        var productNames = products.Select(p => p.Name).ToList();
        Assert.Equal(productNames.Count, productNames.Distinct().Count());
    }

    [Fact]
    public void Seed_AccountsHaveUniqueAccountNumbers()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var accounts = context.Accounts.ToList();
        var accountNumbers = accounts.Select(a => a.AccountNumber).ToList();
        Assert.Equal(accountNumbers.Count, accountNumbers.Distinct().Count());
    }

    [Fact]
    public void Seed_AccountsHaveUniqueEmails()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var accounts = context.Accounts.ToList();
        var emails = accounts.Select(a => a.Email).ToList();
        Assert.Equal(emails.Count, emails.Distinct().Count());
    }

    [Fact]
    public void Seed_ProductsHaveValidInterestRates()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        Assert.All(products, p => Assert.True(p.InterestRate >= 0));
        Assert.All(products, p => Assert.True(p.InterestRate <= 100));
    }

    [Fact]
    public void Seed_ProductsHaveValidAmountRanges()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        Assert.All(products, p => Assert.True(p.MinAmount >= 0));
        Assert.All(products, p => Assert.True(p.MaxAmount >= p.MinAmount));
    }

    [Fact]
    public void Seed_ProductsHaveValidTermRanges()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        DbInitializer.Seed(context);

        // Assert
        var products = context.Products.ToList();
        Assert.All(products, p => Assert.True(p.MinTermMonths >= 0));
        Assert.All(products, p => Assert.True(p.MaxTermMonths >= p.MinTermMonths));
    }
}


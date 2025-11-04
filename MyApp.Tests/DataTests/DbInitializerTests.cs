using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.DataTests;

public class DbInitializerTests
{

    [Fact]
    public void DbInitializer_ShouldCreateDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        Assert.True(context.Database.CanConnect());
    }

    [Fact]
    public void DbInitializer_ShouldSeedProducts()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var products = context.Products.ToList();
        Assert.Equal(4, products.Count);
        var expectedProductNames = new[] { "Personal Loan", "Home Loan", "Credit Card", "Savings Account" };
        Assert.All(expectedProductNames, name => Assert.Contains(products, p => p.Name == name));
    }

    [Theory]
    [InlineData("Personal Loan", "Loan", 5.5, 1000, 50000)]
    [InlineData("Home Loan", "Loan", 4.2, 50000, 500000)]
    [InlineData("Credit Card", "CreditCard", 18.9, 500, 10000)]
    [InlineData("Savings Account", "SavingsAccount", 2.5, 0, 1000000)]
    public void DbInitializer_ShouldSeedProductsWithCorrectData(
        string name, string productType, decimal interestRate, decimal minAmount, decimal maxAmount)
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var product = context.Products.First(p => p.Name == name);
        Assert.Equal(productType, product.ProductType);
        Assert.Equal(interestRate, product.InterestRate);
        Assert.Equal(minAmount, product.MinAmount);
        Assert.Equal(maxAmount, product.MaxAmount);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void DbInitializer_ShouldSeedAccounts()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var accounts = context.Accounts.ToList();
        Assert.Equal(3, accounts.Count);
        var expectedAccountNumbers = new[] { "ACC001", "ACC002", "ACC003" };
        Assert.All(expectedAccountNumbers, num => Assert.Contains(accounts, a => a.AccountNumber == num));
    }

    [Theory]
    [InlineData("ACC001", "John Doe", 15000.00, "Savings")]
    [InlineData("ACC002", "Jane Smith", 8500.50, "Checking")]
    [InlineData("ACC003", "Bob Johnson", 25000.00, "Savings")]
    public void DbInitializer_ShouldSeedAccountsWithCorrectData(
        string accountNumber, string accountHolderName, decimal balance, string accountType)
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        var account = context.Accounts.First(a => a.AccountNumber == accountNumber);
        Assert.Equal(accountHolderName, account.AccountHolderName);
        Assert.Equal(balance, account.Balance);
        Assert.Equal(accountType, account.AccountType);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void DbInitializer_ShouldSeedTransactions()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

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
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        
        // Seed once
        DbInitializer.Initialize(context);
        var initialProductCount = context.Products.Count();

        // Add a product manually
        var testProduct = TestDataFactory.CreateTestProduct(
            name: "Test Product",
            interestRate: 10m,
            minAmount: 100m,
            maxAmount: 1000m,
            description: "Test");
        context.Products.Add(testProduct);
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
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var beforeInit = DateTime.UtcNow;

        // Act
        DbInitializer.Initialize(context);
        var afterInit = DateTime.UtcNow;

        // Assert
        var products = context.Products.ToList();
        Assert.All(products, product =>
        {
            Assert.True(product.CreatedDate >= beforeInit.AddMinutes(-1));
            Assert.True(product.CreatedDate <= afterInit.AddMinutes(1));
        });

        var accounts = context.Accounts.ToList();
        Assert.All(accounts, account =>
        {
            Assert.True(account.CreatedDate >= beforeInit.AddMonths(-12).AddMinutes(-1));
            Assert.True(account.CreatedDate <= afterInit.AddMinutes(1));
        });
    }
}


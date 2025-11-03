using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Data;
using Xunit;

namespace MyApp.Tests;

public class ApplicationDbContextTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void DbContext_CanCreateInstance()
    {
        // Arrange & Act
        using var context = CreateInMemoryDbContext();

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Accounts);
        Assert.NotNull(context.Products);
        Assert.NotNull(context.Transactions);
        Assert.NotNull(context.Applications);
    }

    [Fact]
    public void DbContext_CanAddAccount()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var account = new Account
        {
            AccountNumber = "TEST001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Phone = "+1-555-0000",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.Accounts.Add(account);
        context.SaveChanges();

        // Assert
        var savedAccount = context.Accounts.FirstOrDefault(a => a.AccountNumber == "TEST001");
        Assert.NotNull(savedAccount);
        Assert.Equal("Test", savedAccount.FirstName);
        Assert.Equal("User", savedAccount.LastName);
    }

    [Fact]
    public void DbContext_AccountNumberMustBeUnique()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var account1 = new Account
        {
            AccountNumber = "TEST001",
            FirstName = "Test",
            LastName = "User",
            Email = "test1@example.com",
            Phone = "+1-555-0001",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        var account2 = new Account
        {
            AccountNumber = "TEST001",
            FirstName = "Test2",
            LastName = "User2",
            Email = "test2@example.com",
            Phone = "+1-555-0002",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.Accounts.Add(account1);
        context.SaveChanges();
        context.Accounts.Add(account2);

        // Assert
        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void DbContext_CanAddProduct()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var product = new Product
        {
            Name = "Test Product",
            ProductType = "Loan",
            MinAmount = 1000m,
            MaxAmount = 10000m,
            InterestRate = 5.0m,
            MinTermMonths = 12,
            MaxTermMonths = 60,
            IsActive = true,
            Description = "Test description"
        };

        // Act
        context.Products.Add(product);
        context.SaveChanges();

        // Assert
        var savedProduct = context.Products.FirstOrDefault(p => p.Name == "Test Product");
        Assert.NotNull(savedProduct);
        Assert.Equal(5.0m, savedProduct.InterestRate);
    }

    [Fact]
    public void DbContext_CanAddTransaction()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var account = new Account
        {
            AccountNumber = "ACC001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Phone = "+1-555-0000",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };
        context.Accounts.Add(account);
        context.SaveChanges();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Amount = 1000.00m,
            TransactionType = "Deposit",
            TransactionDate = DateTime.UtcNow,
            Description = "Test transaction"
        };

        // Act
        context.Transactions.Add(transaction);
        context.SaveChanges();

        // Assert
        var savedTransaction = context.Transactions.FirstOrDefault(t => t.AccountId == account.Id);
        Assert.NotNull(savedTransaction);
        Assert.Equal(1000.00m, savedTransaction.Amount);
        Assert.Equal("Deposit", savedTransaction.TransactionType);
    }

    [Fact]
    public void DbContext_TransactionRequiresAccount()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var transaction = new Transaction
        {
            AccountId = 999, // Non-existent account
            Amount = 1000.00m,
            TransactionType = "Deposit",
            TransactionDate = DateTime.UtcNow,
            Description = "Test transaction"
        };

        // Act
        context.Transactions.Add(transaction);

        // Assert
        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void DbContext_CanAddApplication()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var account = new Account
        {
            AccountNumber = "ACC001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Phone = "+1-555-0000",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };
        context.Accounts.Add(account);
        context.SaveChanges();

        var application = new Application
        {
            AccountId = account.Id,
            ApplicationType = "Loan",
            RequestedAmount = 50000m,
            Status = "Pending",
            ApplicationDate = DateTime.UtcNow
        };

        // Act
        context.Applications.Add(application);
        context.SaveChanges();

        // Assert
        var savedApplication = context.Applications.FirstOrDefault(a => a.AccountId == account.Id);
        Assert.NotNull(savedApplication);
        Assert.Equal(50000m, savedApplication.RequestedAmount);
        Assert.Equal("Pending", savedApplication.Status);
    }

    [Fact]
    public void DbContext_AccountPropertiesAreRequired()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var account = new Account
        {
            AccountNumber = null!,
            FirstName = null!,
            LastName = null!,
            Email = null!
        };

        // Act
        context.Accounts.Add(account);

        // Assert
        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void DbContext_AccountHasMaxLengthConstraints()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var account = new Account
        {
            AccountNumber = new string('A', 51), // Exceeds max length of 50
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Phone = "+1-555-0000",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.Accounts.Add(account);

        // Assert
        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void DbContext_OnModelCreating_ConfiguresEntities()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act - Force model creation by accessing it
        var accountEntity = context.Model.FindEntityType(typeof(Account));
        var transactionEntity = context.Model.FindEntityType(typeof(Transaction));
        var applicationEntity = context.Model.FindEntityType(typeof(Application));
        var productEntity = context.Model.FindEntityType(typeof(Product));

        // Assert
        Assert.NotNull(accountEntity);
        Assert.NotNull(transactionEntity);
        Assert.NotNull(applicationEntity);
        Assert.NotNull(productEntity);
        
        // Verify model configuration was applied
        var accountKey = accountEntity!.FindPrimaryKey();
        Assert.NotNull(accountKey);
        Assert.Contains(accountKey.Properties, p => p.Name == "Id");
    }

    [Fact]
    public void DbContext_OnModelCreating_ConfiguresAccountProperties()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        var accountEntity = context.Model.FindEntityType(typeof(Account));
        var accountNumberProperty = accountEntity!.FindProperty("AccountNumber");
        var emailProperty = accountEntity.FindProperty("Email");

        // Assert
        Assert.NotNull(accountNumberProperty);
        Assert.False(accountNumberProperty!.IsNullable);
        Assert.Equal(50, accountNumberProperty.GetMaxLength());
        
        Assert.NotNull(emailProperty);
        Assert.Equal(255, emailProperty!.GetMaxLength());
    }

    [Fact]
    public void DbContext_OnModelCreating_ConfiguresTransactionRelationship()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        var transactionEntity = context.Model.FindEntityType(typeof(Transaction));
        var foreignKey = transactionEntity!.GetForeignKeys().FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Account));

        // Assert
        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey!.DeleteBehavior);
    }

    [Fact]
    public void DbContext_OnModelCreating_ConfiguresApplicationRelationship()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        var applicationEntity = context.Model.FindEntityType(typeof(Application));
        var foreignKey = applicationEntity!.GetForeignKeys().FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Account));

        // Assert
        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey!.DeleteBehavior);
    }

    [Fact]
    public void DbContext_OnModelCreating_ConfiguresDecimalPrecision()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Act
        var transactionEntity = context.Model.FindEntityType(typeof(Transaction));
        var amountProperty = transactionEntity!.FindProperty("Amount");
        var columnType = amountProperty!.GetColumnType();

        // Assert
        Assert.NotNull(columnType);
        Assert.Contains("decimal", columnType);
    }
}


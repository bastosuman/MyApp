using MyApp.Core.Entities;
using Xunit;

namespace MyApp.Tests;

public class AccountEntityTests
{
    [Fact]
    public void Account_CanBeCreated()
    {
        // Arrange & Act
        var account = new Account
        {
            Id = 1,
            AccountNumber = "ACC001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "+1-555-0000",
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        Assert.Equal(1, account.Id);
        Assert.Equal("ACC001", account.AccountNumber);
        Assert.Equal("John", account.FirstName);
        Assert.Equal("Doe", account.LastName);
        Assert.Equal("john@example.com", account.Email);
        Assert.Equal("+1-555-0000", account.Phone);
        Assert.True(account.IsActive);
        Assert.NotNull(account.Transactions);
        Assert.NotNull(account.Applications);
    }

    [Fact]
    public void Account_TransactionsCollectionIsInitialized()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.NotNull(account.Transactions);
        Assert.Empty(account.Transactions);
    }

    [Fact]
    public void Account_ApplicationsCollectionIsInitialized()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.NotNull(account.Applications);
        Assert.Empty(account.Applications);
    }

    [Fact]
    public void Account_AccountNumberDefaultsToEmptyString()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.Equal(string.Empty, account.AccountNumber);
    }

    [Fact]
    public void Account_FirstNameDefaultsToEmptyString()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.Equal(string.Empty, account.FirstName);
    }

    [Fact]
    public void Account_LastNameDefaultsToEmptyString()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.Equal(string.Empty, account.LastName);
    }

    [Fact]
    public void Account_EmailDefaultsToEmptyString()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.Equal(string.Empty, account.Email);
    }

    [Fact]
    public void Account_PhoneDefaultsToEmptyString()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.Equal(string.Empty, account.Phone);
    }

    [Fact]
    public void Account_CanSetAccountNumber()
    {
        // Arrange
        var account = new Account();
        var accountNumber = "ACC123";

        // Act
        account.AccountNumber = accountNumber;

        // Assert
        Assert.Equal(accountNumber, account.AccountNumber);
    }

    [Fact]
    public void Account_CanSetFirstName()
    {
        // Arrange
        var account = new Account();
        var firstName = "John";

        // Act
        account.FirstName = firstName;

        // Assert
        Assert.Equal(firstName, account.FirstName);
    }

    [Fact]
    public void Account_CanSetLastName()
    {
        // Arrange
        var account = new Account();
        var lastName = "Doe";

        // Act
        account.LastName = lastName;

        // Assert
        Assert.Equal(lastName, account.LastName);
    }

    [Fact]
    public void Account_CanSetEmail()
    {
        // Arrange
        var account = new Account();
        var email = "john.doe@example.com";

        // Act
        account.Email = email;

        // Assert
        Assert.Equal(email, account.Email);
    }

    [Fact]
    public void Account_CanSetPhone()
    {
        // Arrange
        var account = new Account();
        var phone = "+1-555-1234";

        // Act
        account.Phone = phone;

        // Assert
        Assert.Equal(phone, account.Phone);
    }
}

public class ProductEntityTests
{
    [Fact]
    public void Product_CanBeCreated()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Name = "Personal Loan",
            ProductType = "Loan",
            MinAmount = 1000m,
            MaxAmount = 50000m,
            InterestRate = 5.5m,
            MinTermMonths = 12,
            MaxTermMonths = 60,
            IsActive = true,
            Description = "Test description"
        };

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("Personal Loan", product.Name);
        Assert.Equal("Loan", product.ProductType);
        Assert.Equal(1000m, product.MinAmount);
        Assert.Equal(50000m, product.MaxAmount);
        Assert.Equal(5.5m, product.InterestRate);
        Assert.Equal(12, product.MinTermMonths);
        Assert.Equal(60, product.MaxTermMonths);
        Assert.True(product.IsActive);
        Assert.Equal("Test description", product.Description);
    }
}

public class TransactionEntityTests
{
    [Fact]
    public void Transaction_CanBeCreated()
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            Id = 1,
            AccountId = 1,
            TransactionType = "Deposit",
            Amount = 1000.00m,
            TransactionDate = DateTime.UtcNow,
            Description = "Test transaction",
            Status = "Completed"
        };

        // Assert
        Assert.Equal(1, transaction.Id);
        Assert.Equal(1, transaction.AccountId);
        Assert.Equal("Deposit", transaction.TransactionType);
        Assert.Equal(1000.00m, transaction.Amount);
        Assert.Equal("Test transaction", transaction.Description);
        Assert.Equal("Completed", transaction.Status);
    }

    [Fact]
    public void Transaction_AccountNavigationPropertyIsNullable()
    {
        // Arrange
        var transaction = new Transaction();

        // Act & Assert
        Assert.Null(transaction.Account);
    }

    [Fact]
    public void Transaction_TransactionTypeDefaultsToEmptyString()
    {
        // Arrange
        var transaction = new Transaction();

        // Act & Assert
        Assert.Equal(string.Empty, transaction.TransactionType);
    }

    [Fact]
    public void Transaction_DescriptionDefaultsToEmptyString()
    {
        // Arrange
        var transaction = new Transaction();

        // Act & Assert
        Assert.Equal(string.Empty, transaction.Description);
    }

    [Fact]
    public void Transaction_StatusDefaultsToEmptyString()
    {
        // Arrange
        var transaction = new Transaction();

        // Act & Assert
        Assert.Equal(string.Empty, transaction.Status);
    }

    [Fact]
    public void Transaction_CanSetTransactionType()
    {
        // Arrange
        var transaction = new Transaction();
        var transactionType = "Withdrawal";

        // Act
        transaction.TransactionType = transactionType;

        // Assert
        Assert.Equal(transactionType, transaction.TransactionType);
    }

    [Fact]
    public void Transaction_CanSetDescription()
    {
        // Arrange
        var transaction = new Transaction();
        var description = "Payment for services";

        // Act
        transaction.Description = description;

        // Assert
        Assert.Equal(description, transaction.Description);
    }

    [Fact]
    public void Transaction_CanSetStatus()
    {
        // Arrange
        var transaction = new Transaction();
        var status = "Pending";

        // Act
        transaction.Status = status;

        // Assert
        Assert.Equal(status, transaction.Status);
    }
}

public class ApplicationEntityTests
{
    [Fact]
    public void Application_CanBeCreated()
    {
        // Arrange & Act
        var application = new Application
        {
            Id = 1,
            AccountId = 1,
            ApplicationType = "Loan",
            RequestedAmount = 50000m,
            ApprovedAmount = 45000m,
            InterestRate = 4.5m,
            TermMonths = 60,
            Status = "Approved",
            ApplicationDate = DateTime.UtcNow,
            DecisionDate = DateTime.UtcNow.AddDays(1),
            Notes = "Test notes"
        };

        // Assert
        Assert.Equal(1, application.Id);
        Assert.Equal(1, application.AccountId);
        Assert.Equal("Loan", application.ApplicationType);
        Assert.Equal(50000m, application.RequestedAmount);
        Assert.Equal(45000m, application.ApprovedAmount);
        Assert.Equal(4.5m, application.InterestRate);
        Assert.Equal(60, application.TermMonths);
        Assert.Equal("Approved", application.Status);
        Assert.NotNull(application.DecisionDate);
        Assert.Equal("Test notes", application.Notes);
    }

    [Fact]
    public void Application_CanBeCreatedWithoutApprovedAmount()
    {
        // Arrange & Act
        var application = new Application
        {
            AccountId = 1,
            ApplicationType = "Loan",
            RequestedAmount = 50000m,
            ApprovedAmount = null,
            Status = "Pending"
        };

        // Assert
        Assert.Null(application.ApprovedAmount);
        Assert.Equal("Pending", application.Status);
    }

    [Fact]
    public void Application_AccountNavigationPropertyIsNullable()
    {
        // Arrange
        var application = new Application();

        // Act & Assert
        Assert.Null(application.Account);
    }
}


using MyApp.Core.Entities;

namespace MyApp.Tests.EntityTests;

public class AccountTests
{
    [Fact]
    public void Account_ShouldInitializeWithDefaultValues()
    {
        // Act
        var account = new Account();

        // Assert
        Assert.Equal(0, account.Id);
        Assert.Equal(string.Empty, account.AccountNumber);
        Assert.Equal(string.Empty, account.AccountHolderName);
        Assert.Equal(0m, account.Balance);
        Assert.Equal(string.Empty, account.AccountType);
        Assert.Equal(DateTime.MinValue, account.CreatedDate);
        Assert.True(account.IsActive);
        Assert.NotNull(account.Transactions);
        Assert.NotNull(account.Applications);
    }

    [Fact]
    public void Account_ShouldSetAndGetProperties()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            AccountNumber = "ACC001",
            AccountHolderName = "John Doe",
            Balance = 15000.00m,
            AccountType = "Savings",
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        Assert.Equal(1, account.Id);
        Assert.Equal("ACC001", account.AccountNumber);
        Assert.Equal("John Doe", account.AccountHolderName);
        Assert.Equal(15000.00m, account.Balance);
        Assert.Equal("Savings", account.AccountType);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void Account_ShouldSupportTransactionsCollection()
    {
        // Arrange
        var account = new Account { Id = 1 };
        var transaction = new Transaction { Id = 1, AccountId = 1 };

        // Act
        account.Transactions.Add(transaction);

        // Assert
        Assert.Single(account.Transactions);
        Assert.Equal(transaction, account.Transactions.First());
    }

    [Fact]
    public void Account_ShouldSupportApplicationsCollection()
    {
        // Arrange
        var account = new Account { Id = 1 };
        var application = new Application { Id = 1, AccountId = 1 };

        // Act
        account.Applications.Add(application);

        // Assert
        Assert.Single(account.Applications);
        Assert.Equal(application, account.Applications.First());
    }

    [Fact]
    public void Account_CanBeInactive()
    {
        // Arrange
        var account = new Account { IsActive = false };

        // Assert
        Assert.False(account.IsActive);
    }
}


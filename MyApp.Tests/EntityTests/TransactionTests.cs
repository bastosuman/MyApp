using MyApp.Core.Entities;

namespace MyApp.Tests.EntityTests;

public class TransactionTests
{
    [Fact]
    public void Transaction_ShouldInitializeWithDefaultValues()
    {
        // Act
        var transaction = new Transaction();

        // Assert
        Assert.Equal(0, transaction.Id);
        Assert.Equal(0, transaction.AccountId);
        Assert.Equal(string.Empty, transaction.TransactionType);
        Assert.Equal(0m, transaction.Amount);
        Assert.Equal(string.Empty, transaction.Description);
        Assert.Equal(DateTime.MinValue, transaction.TransactionDate);
        Assert.Equal("Completed", transaction.Status);
        // Account navigation property is initialized but may be null until loaded
    }

    [Fact]
    public void Transaction_ShouldSetAndGetProperties()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = 1,
            AccountId = 1,
            TransactionType = "Deposit",
            Amount = 1000.00m,
            Description = "Initial deposit",
            TransactionDate = DateTime.UtcNow,
            Status = "Completed"
        };

        // Assert
        Assert.Equal(1, transaction.Id);
        Assert.Equal(1, transaction.AccountId);
        Assert.Equal("Deposit", transaction.TransactionType);
        Assert.Equal(1000.00m, transaction.Amount);
        Assert.Equal("Initial deposit", transaction.Description);
        Assert.Equal("Completed", transaction.Status);
    }

    [Theory]
    [InlineData("Deposit")]
    [InlineData("Withdrawal")]
    [InlineData("Transfer")]
    public void Transaction_ShouldSupportDifferentTransactionTypes(string transactionType)
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionType = transactionType
        };

        // Assert
        Assert.Equal(transactionType, transaction.TransactionType);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Pending")]
    [InlineData("Failed")]
    public void Transaction_ShouldSupportDifferentStatuses(string status)
    {
        // Arrange
        var transaction = new Transaction
        {
            Status = status
        };

        // Assert
        Assert.Equal(status, transaction.Status);
    }

    [Fact]
    public void Transaction_ShouldHaveDefaultStatusAsCompleted()
    {
        // Act
        var transaction = new Transaction();

        // Assert
        Assert.Equal("Completed", transaction.Status);
    }
}


using MyApp.Core.Entities;

namespace MyApp.Tests.TestHelpers;

/// <summary>
/// Factory for creating test data entities
/// </summary>
public static class TestDataFactory
{
    public static Account CreateTestAccount(
        string accountNumber = "ACC001",
        string accountHolderName = "John Doe",
        decimal balance = 1000m,
        string accountType = "Savings",
        bool isActive = true)
    {
        return new Account
        {
            AccountNumber = accountNumber,
            AccountHolderName = accountHolderName,
            Balance = balance,
            AccountType = accountType,
            CreatedDate = DateTime.UtcNow,
            IsActive = isActive
        };
    }

    public static Product CreateTestProduct(
        string name = "Personal Loan",
        string productType = "Loan",
        decimal interestRate = 5.5m,
        decimal minAmount = 1000m,
        decimal maxAmount = 50000m,
        string description = "Test product",
        bool isActive = true)
    {
        return new Product
        {
            Name = name,
            ProductType = productType,
            InterestRate = interestRate,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Description = description,
            IsActive = isActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static Transaction CreateTestTransaction(
        int accountId,
        string transactionType = "Deposit",
        decimal amount = 500m,
        string description = "Test transaction",
        string status = "Completed")
    {
        return new Transaction
        {
            AccountId = accountId,
            TransactionType = transactionType,
            Amount = amount,
            Description = description,
            TransactionDate = DateTime.UtcNow,
            Status = status
        };
    }

    public static Application CreateTestApplication(
        int accountId,
        int productId,
        decimal requestedAmount = 10000m,
        string status = "Pending")
    {
        return new Application
        {
            AccountId = accountId,
            ProductId = productId,
            RequestedAmount = requestedAmount,
            Status = status,
            ApplicationDate = DateTime.UtcNow
        };
    }
}


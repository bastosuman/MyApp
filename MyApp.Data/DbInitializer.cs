using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;

namespace MyApp.Data;

public static class DbInitializer
{
    public static void Initialize(FinancialDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Ensure database is created (should already exist from migration)
        context.Database.EnsureCreated();

        // Check if database already has data
        if (context.Products.Any())
        {
            return; // Database has been seeded
        }

        // Seed Products
        var products = CreateSeedProducts();

        context.Products.AddRange(products);
        context.SaveChanges();

        // Seed Accounts
        var accounts = CreateSeedAccounts();

        context.Accounts.AddRange(accounts);
        context.SaveChanges();

        // Seed some sample transactions
        var transactions = CreateSeedTransactions(accounts);

        context.Transactions.AddRange(transactions);
        context.SaveChanges();
    }

    private static Product[] CreateSeedProducts()
    {
        var now = DateTime.UtcNow;
        return new Product[]
        {
            CreateProduct("Personal Loan", "Loan", 5.5m, 1000m, 50000m, "Personal loan for individual borrowers with flexible repayment terms", now),
            CreateProduct("Home Loan", "Loan", 4.2m, 50000m, 500000m, "Home mortgage loan for property purchase or refinancing", now),
            CreateProduct("Credit Card", "CreditCard", 18.9m, 500m, 10000m, "Credit card with competitive interest rates and rewards program", now),
            CreateProduct("Savings Account", "SavingsAccount", 2.5m, 0m, 1000000m, "High-yield savings account with competitive interest rates", now)
        };
    }

    private static Product CreateProduct(string name, string productType, decimal interestRate, decimal minAmount, decimal maxAmount, string description, DateTime createdDate)
    {
        return new Product
        {
            Name = name,
            ProductType = productType,
            InterestRate = interestRate,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Description = description,
            IsActive = true,
            CreatedDate = createdDate
        };
    }

    private static Account[] CreateSeedAccounts()
    {
        var now = DateTime.UtcNow;
        return new Account[]
        {
            CreateAccount("ACC001", "John Doe", 15000.00m, "Savings", now.AddMonths(-6)),
            CreateAccount("ACC002", "Jane Smith", 8500.50m, "Checking", now.AddMonths(-3)),
            CreateAccount("ACC003", "Bob Johnson", 25000.00m, "Savings", now.AddMonths(-12))
        };
    }

    private static Account CreateAccount(string accountNumber, string accountHolderName, decimal balance, string accountType, DateTime createdDate)
    {
        return new Account
        {
            AccountNumber = accountNumber,
            AccountHolderName = accountHolderName,
            Balance = balance,
            AccountType = accountType,
            CreatedDate = createdDate,
            IsActive = true
        };
    }

    private static Transaction[] CreateSeedTransactions(Account[] accounts)
    {
        var now = DateTime.UtcNow;
        return new Transaction[]
        {
            CreateTransaction(accounts[0].Id, "Deposit", 5000.00m, "Initial deposit", now.AddMonths(-6)),
            CreateTransaction(accounts[0].Id, "Withdrawal", 500.00m, "ATM withdrawal", now.AddMonths(-5)),
            CreateTransaction(accounts[1].Id, "Deposit", 10000.00m, "Salary deposit", now.AddMonths(-3)),
            CreateTransaction(accounts[1].Id, "Withdrawal", 1500.50m, "Purchase payment", now.AddMonths(-2))
        };
    }

    private static Transaction CreateTransaction(int accountId, string transactionType, decimal amount, string description, DateTime transactionDate)
    {
        return new Transaction
        {
            AccountId = accountId,
            TransactionType = transactionType,
            Amount = amount,
            Description = description,
            TransactionDate = transactionDate,
            Status = "Completed"
        };
    }
}


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
        var products = new Product[]
        {
            new Product
            {
                Name = "Personal Loan",
                ProductType = "Loan",
                InterestRate = 5.5m,
                MinAmount = 1000m,
                MaxAmount = 50000m,
                Description = "Personal loan for individual borrowers with flexible repayment terms",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "Home Loan",
                ProductType = "Loan",
                InterestRate = 4.2m,
                MinAmount = 50000m,
                MaxAmount = 500000m,
                Description = "Home mortgage loan for property purchase or refinancing",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "Credit Card",
                ProductType = "CreditCard",
                InterestRate = 18.9m,
                MinAmount = 500m,
                MaxAmount = 10000m,
                Description = "Credit card with competitive interest rates and rewards program",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "Savings Account",
                ProductType = "SavingsAccount",
                InterestRate = 2.5m,
                MinAmount = 0m,
                MaxAmount = 1000000m,
                Description = "High-yield savings account with competitive interest rates",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Seed Accounts
        var accounts = new Account[]
        {
            new Account
            {
                AccountNumber = "ACC001",
                AccountHolderName = "John Doe",
                Balance = 15000.00m,
                AccountType = "Savings",
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                IsActive = true
            },
            new Account
            {
                AccountNumber = "ACC002",
                AccountHolderName = "Jane Smith",
                Balance = 8500.50m,
                AccountType = "Checking",
                CreatedDate = DateTime.UtcNow.AddMonths(-3),
                IsActive = true
            },
            new Account
            {
                AccountNumber = "ACC003",
                AccountHolderName = "Bob Johnson",
                Balance = 25000.00m,
                AccountType = "Savings",
                CreatedDate = DateTime.UtcNow.AddMonths(-12),
                IsActive = true
            }
        };

        context.Accounts.AddRange(accounts);
        context.SaveChanges();

        // Seed some sample transactions
        var transactions = new Transaction[]
        {
            new Transaction
            {
                AccountId = accounts[0].Id,
                TransactionType = "Deposit",
                Amount = 5000.00m,
                Description = "Initial deposit",
                TransactionDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Completed"
            },
            new Transaction
            {
                AccountId = accounts[0].Id,
                TransactionType = "Withdrawal",
                Amount = 500.00m,
                Description = "ATM withdrawal",
                TransactionDate = DateTime.UtcNow.AddMonths(-5),
                Status = "Completed"
            },
            new Transaction
            {
                AccountId = accounts[1].Id,
                TransactionType = "Deposit",
                Amount = 10000.00m,
                Description = "Salary deposit",
                TransactionDate = DateTime.UtcNow.AddMonths(-3),
                Status = "Completed"
            },
            new Transaction
            {
                AccountId = accounts[1].Id,
                TransactionType = "Withdrawal",
                Amount = 1500.50m,
                Description = "Purchase payment",
                TransactionDate = DateTime.UtcNow.AddMonths(-2),
                Status = "Completed"
            }
        };

        context.Transactions.AddRange(transactions);
        context.SaveChanges();
    }
}


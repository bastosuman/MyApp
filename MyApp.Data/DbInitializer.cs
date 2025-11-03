using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;

namespace MyApp.Data;

/// <summary>
/// Database initializer for seeding initial data
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with initial data if it doesn't already exist
    /// </summary>
    public static void Seed(ApplicationDbContext context)
    {
        // Check if data already exists (skip seeding if already populated)
        if (context.Products.Any() || context.Accounts.Any())
        {
            return; // Database already seeded
        }

        // Seed Products
        var products = new List<Product>
        {
            new Product
            {
                Name = "Personal Loan",
                ProductType = "Loan",
                MinAmount = 1000.00m,
                MaxAmount = 50000.00m,
                InterestRate = 5.5m,
                MinTermMonths = 12,
                MaxTermMonths = 60,
                IsActive = true,
                Description = "Flexible personal loans for various needs with competitive rates."
            },
            new Product
            {
                Name = "Home Loan",
                ProductType = "Mortgage",
                MinAmount = 50000.00m,
                MaxAmount = 500000.00m,
                InterestRate = 4.2m,
                MinTermMonths = 60,
                MaxTermMonths = 360,
                IsActive = true,
                Description = "Mortgage loans for home purchase or refinancing with attractive rates."
            },
            new Product
            {
                Name = "Credit Card",
                ProductType = "Credit",
                MinAmount = 500.00m,
                MaxAmount = 10000.00m,
                InterestRate = 18.9m,
                MinTermMonths = 1,
                MaxTermMonths = 60,
                IsActive = true,
                Description = "Credit card with flexible repayment options and rewards program."
            },
            new Product
            {
                Name = "Savings Account",
                ProductType = "Deposit",
                MinAmount = 0.00m,
                MaxAmount = 1000000.00m,
                InterestRate = 2.5m,
                MinTermMonths = 0,
                MaxTermMonths = 0,
                IsActive = true,
                Description = "High-yield savings account with competitive interest rates."
            }
        };

        context.Products.AddRange(products);

        // Seed Accounts
        var accounts = new List<Account>
        {
            new Account
            {
                AccountNumber = "ACC001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "+1-555-0101",
                DateCreated = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new Account
            {
                AccountNumber = "ACC002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Phone = "+1-555-0102",
                DateCreated = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            },
            new Account
            {
                AccountNumber = "ACC003",
                FirstName = "Robert",
                LastName = "Johnson",
                Email = "robert.johnson@example.com",
                Phone = "+1-555-0103",
                DateCreated = DateTime.UtcNow.AddDays(-7),
                IsActive = true
            }
        };

        context.Accounts.AddRange(accounts);

        // Save all changes
        context.SaveChanges();
    }
}


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
        SeedProducts(context);
        SeedAccounts(context);
        context.SaveChanges();
    }

    private static void SeedProducts(ApplicationDbContext context)
    {
        if (context.Products.Any()) return;

        var products = GetSeedProducts();
        context.Products.AddRange(products);
    }

    private static void SeedAccounts(ApplicationDbContext context)
    {
        if (context.Accounts.Any()) return;

        var accounts = GetSeedAccounts();
        context.Accounts.AddRange(accounts);
    }

    private static List<Product> GetSeedProducts()
    {
        return new List<Product>
        {
            CreateProduct("Personal Loan", "Loan", 1000.00m, 50000.00m, 5.5m, 12, 60, 
                "Flexible personal loans for various needs with competitive rates."),
            CreateProduct("Home Loan", "Mortgage", 50000.00m, 500000.00m, 4.2m, 60, 360, 
                "Mortgage loans for home purchase or refinancing with attractive rates."),
            CreateProduct("Credit Card", "Credit", 500.00m, 10000.00m, 18.9m, 1, 60, 
                "Credit card with flexible repayment options and rewards program."),
            CreateProduct("Savings Account", "Deposit", 0.00m, 1000000.00m, 2.5m, 0, 0, 
                "High-yield savings account with competitive interest rates.")
        };
    }

    private static List<Account> GetSeedAccounts()
    {
        return new List<Account>
        {
            CreateAccount("ACC001", "John", "Doe", "john.doe@example.com", "+1-555-0101", -30),
            CreateAccount("ACC002", "Jane", "Smith", "jane.smith@example.com", "+1-555-0102", -15),
            CreateAccount("ACC003", "Robert", "Johnson", "robert.johnson@example.com", "+1-555-0103", -7)
        };
    }

    private static Product CreateProduct(string name, string productType, decimal minAmount, 
        decimal maxAmount, decimal interestRate, int minTermMonths, int maxTermMonths, string description)
    {
        return new Product
        {
            Name = name,
            ProductType = productType,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            InterestRate = interestRate,
            MinTermMonths = minTermMonths,
            MaxTermMonths = maxTermMonths,
            IsActive = true,
            Description = description
        };
    }

    private static Account CreateAccount(string accountNumber, string firstName, string lastName, 
        string email, string phone, int daysOffset)
    {
        return new Account
        {
            AccountNumber = accountNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            DateCreated = DateTime.UtcNow.AddDays(daysOffset),
            IsActive = true
        };
    }
}


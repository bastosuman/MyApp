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
            CreateProduct(new ProductConfig("Personal Loan", "Loan", 1000.00m, 50000.00m, 5.5m, 12, 60, 
                "Flexible personal loans for various needs with competitive rates.")),
            CreateProduct(new ProductConfig("Home Loan", "Mortgage", 50000.00m, 500000.00m, 4.2m, 60, 360, 
                "Mortgage loans for home purchase or refinancing with attractive rates.")),
            CreateProduct(new ProductConfig("Credit Card", "Credit", 500.00m, 10000.00m, 18.9m, 1, 60, 
                "Credit card with flexible repayment options and rewards program.")),
            CreateProduct(new ProductConfig("Savings Account", "Deposit", 0.00m, 1000000.00m, 2.5m, 0, 0, 
                "High-yield savings account with competitive interest rates."))
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

    private static Product CreateProduct(ProductConfig config)
    {
        return new Product
        {
            Name = config.Name,
            ProductType = config.ProductType,
            MinAmount = config.MinAmount,
            MaxAmount = config.MaxAmount,
            InterestRate = config.InterestRate,
            MinTermMonths = config.MinTermMonths,
            MaxTermMonths = config.MaxTermMonths,
            IsActive = true,
            Description = config.Description
        };
    }

    private sealed record ProductConfig(
        string Name,
        string ProductType,
        decimal MinAmount,
        decimal MaxAmount,
        decimal InterestRate,
        int MinTermMonths,
        int MaxTermMonths,
        string Description);

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


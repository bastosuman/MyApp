using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;

namespace MyApp.Data;

/// <summary>
/// Entity Framework DbContext for the financial services application
/// </summary>
public class ApplicationDbContext : DbContext
{
    private const string DecimalTypeStandard = "decimal(18,2)";
    private const string DecimalTypeInterestRate = "decimal(5,2)";
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Account entity
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasIndex(e => e.Email);
        });
        
        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType(DecimalTypeStandard);
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure Application entity
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestedAmount).HasColumnType(DecimalTypeStandard);
            entity.Property(e => e.ApprovedAmount).HasColumnType(DecimalTypeStandard);
            entity.Property(e => e.InterestRate).HasColumnType(DecimalTypeInterestRate);
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.Applications)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MinAmount).HasColumnType(DecimalTypeStandard);
            entity.Property(e => e.MaxAmount).HasColumnType(DecimalTypeStandard);
            entity.Property(e => e.InterestRate).HasColumnType(DecimalTypeInterestRate);
        });
    }
}



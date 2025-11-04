using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyApp.Data;

public class FinancialDbContextFactory : IDesignTimeDbContextFactory<FinancialDbContext>
{
    public FinancialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinancialDbContext>();
        
        // For design-time migrations, use connection string with TrustServerCertificate for local development
        var connectionString = "Server=localhost;Database=MyAppFinancial;Trusted_Connection=True;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new FinancialDbContext(optionsBuilder.Options);
    }
}


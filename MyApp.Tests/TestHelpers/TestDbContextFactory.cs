using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace MyApp.Tests.TestHelpers;

/// <summary>
/// Shared test helper for creating in-memory database contexts
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static FinancialDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new FinancialDbContext(options);
    }
}


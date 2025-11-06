using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in transfer query patterns
/// </summary>
public static class TransferQueryHelper
{
    /// <summary>
    /// Gets a transfer with all related entities included
    /// </summary>
    public static IQueryable<Transfer> GetTransferWithIncludes(FinancialDbContext context)
    {
        return context.Transfers
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount);
    }

    /// <summary>
    /// Gets scheduled transfers with all related entities included
    /// </summary>
    public static IQueryable<ScheduledTransfer> GetScheduledTransferWithIncludes(FinancialDbContext context)
    {
        return context.ScheduledTransfers
            .Include(s => s.SourceAccount)
            .Include(s => s.DestinationAccount);
    }
}


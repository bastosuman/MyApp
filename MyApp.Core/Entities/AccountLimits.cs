namespace MyApp.Core.Entities;

public class AccountLimits
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal DailyTransferLimit { get; set; } = 10000m; // Default $10,000
    public decimal MonthlyTransferLimit { get; set; } = 50000m; // Default $50,000
    public decimal PerTransactionMax { get; set; } = 5000m; // Default $5,000
    public decimal PerTransactionMin { get; set; } = 1m; // Default $1.00
    public DateTime? LastDailyReset { get; set; }
    public DateTime? LastMonthlyReset { get; set; }
    public decimal DailyTransferUsed { get; set; } = 0m;
    public decimal MonthlyTransferUsed { get; set; } = 0m;
    
    // Navigation property
    public virtual Account Account { get; set; } = null!;
}



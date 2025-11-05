namespace MyApp.Core.DTOs;

public class DashboardDto
{
    public AccountSummaryDto AccountSummary { get; set; } = new();
    public List<TransactionDto> RecentTransactions { get; set; } = new();
    public ApplicationStatusDto ApplicationStatus { get; set; } = new();
    public int AvailableProductsCount { get; set; }
}

public class AccountSummaryDto
{
    public decimal TotalBalance { get; set; }
    public int AccountCount { get; set; }
    public List<AccountDto> Accounts { get; set; } = new();
}

public class ApplicationStatusDto
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Total { get; set; }
}


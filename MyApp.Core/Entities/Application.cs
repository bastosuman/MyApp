namespace MyApp.Core.Entities;

public class Application
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int ProductId { get; set; }
    public decimal RequestedAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed
    public DateTime ApplicationDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Account Account { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}


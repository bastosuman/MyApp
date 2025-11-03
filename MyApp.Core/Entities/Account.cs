namespace MyApp.Core.Entities;

/// <summary>
/// Represents a user account in the financial services platform
/// </summary>
public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}



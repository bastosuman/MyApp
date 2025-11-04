using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
}

public class CreateTransactionDto
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}


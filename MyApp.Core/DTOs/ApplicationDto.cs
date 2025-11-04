using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.DTOs;

public class ApplicationDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int ProductId { get; set; }
    public decimal RequestedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? Notes { get; set; }
    public string? AccountNumber { get; set; }
    public string? ProductName { get; set; }
}

public class CreateApplicationDto
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Requested amount must be greater than zero")]
    public decimal RequestedAmount { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateApplicationStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}


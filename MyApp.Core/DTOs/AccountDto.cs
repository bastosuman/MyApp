using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.DTOs;

public class AccountDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAccountDto
{
    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string AccountHolderName { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative")]
    public decimal Balance { get; set; }

    [Required]
    [StringLength(50)]
    public string AccountType { get; set; } = string.Empty;
}

public class UpdateAccountDto
{
    [Required]
    [StringLength(200)]
    public string AccountHolderName { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative")]
    public decimal Balance { get; set; }

    [Required]
    [StringLength(50)]
    public string AccountType { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}


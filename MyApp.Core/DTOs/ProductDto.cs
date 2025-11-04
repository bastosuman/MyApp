using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ProductType { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
    public decimal InterestRate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Min amount must be non-negative")]
    public decimal MinAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Max amount must be non-negative")]
    public decimal MaxAmount { get; set; }

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}


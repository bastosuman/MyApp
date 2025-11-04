using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Data;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(FinancialDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all active products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.Products.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            var products = await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProductType = p.ProductType,
                    InterestRate = p.InterestRate,
                    MinAmount = p.MinAmount,
                    MaxAmount = p.MaxAmount,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate
                })
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("An error occurred while retrieving products"));
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
    {
        try
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProductType = p.ProductType,
                    InterestRate = p.InterestRate,
                    MinAmount = p.MinAmount,
                    MaxAmount = p.MaxAmount,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Product with ID {id} not found"));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse("An error occurred while retrieving the product"));
        }
    }
}


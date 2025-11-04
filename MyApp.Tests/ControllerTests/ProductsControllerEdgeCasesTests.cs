using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class ProductsControllerEdgeCasesTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ProductDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllProducts_WhenAllAreInactive()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        var inactiveProduct1 = TestDataFactory.CreateTestProduct("Inactive Product 1", isActive: false);
        var inactiveProduct2 = TestDataFactory.CreateTestProduct("Inactive Product 2", isActive: false);
        context.Products.AddRange(inactiveProduct1, inactiveProduct2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProducts(includeInactive: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ProductDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var products = response.Data.ToList();
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnEmptyList_WhenOnlyInactiveProductsExist_AndIncludeInactiveIsFalse()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        var inactiveProduct = TestDataFactory.CreateTestProduct("Inactive Product", isActive: false);
        context.Products.Add(inactiveProduct);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProducts(includeInactive: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ProductDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }
}


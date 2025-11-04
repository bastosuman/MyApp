using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class ProductsControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnActiveProducts_ByDefault()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        var activeProduct = TestDataFactory.CreateTestProduct("Active Product", isActive: true);
        var inactiveProduct = TestDataFactory.CreateTestProduct("Inactive Product", isActive: false);
        context.Products.AddRange(activeProduct, inactiveProduct);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ProductDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        var products = response.Data.ToList();
        Assert.Single(products);
        Assert.Equal("Active Product", products[0].Name);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllProducts_WhenIncludeInactiveIsTrue()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        var activeProduct = TestDataFactory.CreateTestProduct("Active Product", isActive: true);
        var inactiveProduct = TestDataFactory.CreateTestProduct("Inactive Product", isActive: false);
        context.Products.AddRange(activeProduct, inactiveProduct);
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
    public async Task GetProduct_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        var product = TestDataFactory.CreateTestProduct("Personal Loan", interestRate: 5.5m);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProduct(product.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(product.Name, response.Data.Name);
        Assert.Equal(product.InterestRate, response.Data.InterestRate);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.GetProduct(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }
}


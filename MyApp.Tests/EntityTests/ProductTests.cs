using MyApp.Core.Entities;

namespace MyApp.Tests.EntityTests;

public class ProductTests
{
    [Fact]
    public void Product_ShouldInitializeWithDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.Equal(0, product.Id);
        Assert.Equal(string.Empty, product.Name);
        Assert.Equal(string.Empty, product.ProductType);
        Assert.Equal(0m, product.InterestRate);
        Assert.Equal(0m, product.MinAmount);
        Assert.Equal(0m, product.MaxAmount);
        Assert.Equal(string.Empty, product.Description);
        Assert.True(product.IsActive);
        Assert.Equal(DateTime.MinValue, product.CreatedDate);
        Assert.NotNull(product.Applications);
    }

    [Fact]
    public void Product_ShouldSetAndGetProperties()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Personal Loan",
            ProductType = "Loan",
            InterestRate = 5.5m,
            MinAmount = 1000m,
            MaxAmount = 50000m,
            Description = "Personal loan product",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("Personal Loan", product.Name);
        Assert.Equal("Loan", product.ProductType);
        Assert.Equal(5.5m, product.InterestRate);
        Assert.Equal(1000m, product.MinAmount);
        Assert.Equal(50000m, product.MaxAmount);
        Assert.Equal("Personal loan product", product.Description);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Product_ShouldSupportApplicationsCollection()
    {
        // Arrange
        var product = new Product { Id = 1 };
        var application = new Application { Id = 1, ProductId = 1 };

        // Act
        product.Applications.Add(application);

        // Assert
        Assert.Single(product.Applications);
        Assert.Equal(application, product.Applications.First());
    }

    [Fact]
    public void Product_CanBeInactive()
    {
        // Arrange
        var product = new Product { IsActive = false };

        // Assert
        Assert.False(product.IsActive);
    }

    [Theory]
    [InlineData("Loan", 5.5)]
    [InlineData("CreditCard", 18.9)]
    [InlineData("SavingsAccount", 2.5)]
    public void Product_ShouldSupportDifferentProductTypes(string productType, decimal interestRate)
    {
        // Arrange
        var product = new Product
        {
            ProductType = productType,
            InterestRate = interestRate
        };

        // Assert
        Assert.Equal(productType, product.ProductType);
        Assert.Equal(interestRate, product.InterestRate);
    }
}


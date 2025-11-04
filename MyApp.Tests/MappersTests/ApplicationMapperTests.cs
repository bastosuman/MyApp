using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Core.Mappers;

namespace MyApp.Tests.MappersTests;

public class ApplicationMapperTests
{
    [Fact]
    public void ToDto_ShouldMapApplicationToDto()
    {
        // Arrange
        var application = new Application
        {
            Id = 1,
            AccountId = 10,
            ProductId = 20,
            RequestedAmount = 5000m,
            Status = "Pending",
            ApplicationDate = DateTime.UtcNow,
            DecisionDate = null,
            Notes = "Test notes"
        };

        // Act
        var dto = application.ToDto();

        // Assert
        Assert.Equal(application.Id, dto.Id);
        Assert.Equal(application.AccountId, dto.AccountId);
        Assert.Equal(application.ProductId, dto.ProductId);
        Assert.Equal(application.RequestedAmount, dto.RequestedAmount);
        Assert.Equal(application.Status, dto.Status);
        Assert.Equal(application.ApplicationDate, dto.ApplicationDate);
        Assert.Equal(application.DecisionDate, dto.DecisionDate);
        Assert.Equal(application.Notes, dto.Notes);
    }

    [Fact]
    public void ToDto_ShouldHandleNullNavigationProperties()
    {
        // Arrange
        var application = new Application
        {
            Id = 1,
            AccountId = 10,
            ProductId = 20,
            RequestedAmount = 5000m,
            Status = "Pending",
            ApplicationDate = DateTime.UtcNow
        };

        // Act
        var dto = application.ToDto();

        // Assert
        Assert.Null(dto.AccountNumber);
        Assert.Null(dto.ProductName);
    }

    [Fact]
    public void ToDto_ShouldMapNavigationProperties_WhenPresent()
    {
        // Arrange
        var account = new Account { AccountNumber = "ACC001" };
        var product = new Product { Name = "Personal Loan" };
        var application = new Application
        {
            Id = 1,
            AccountId = 10,
            ProductId = 20,
            RequestedAmount = 5000m,
            Status = "Pending",
            ApplicationDate = DateTime.UtcNow,
            Account = account,
            Product = product
        };

        // Act
        var dto = application.ToDto();

        // Assert
        Assert.Equal("ACC001", dto.AccountNumber);
        Assert.Equal("Personal Loan", dto.ProductName);
    }
}


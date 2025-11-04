using MyApp.Core.Entities;

namespace MyApp.Tests.EntityTests;

public class ApplicationTests
{
    [Fact]
    public void Application_ShouldInitializeWithDefaultValues()
    {
        // Act
        var application = new Application();

        // Assert
        Assert.Equal(0, application.Id);
        Assert.Equal(0, application.AccountId);
        Assert.Equal(0, application.ProductId);
        Assert.Equal(0m, application.RequestedAmount);
        Assert.Equal("Pending", application.Status);
        Assert.Equal(DateTime.MinValue, application.ApplicationDate);
        Assert.Null(application.DecisionDate);
        Assert.Null(application.Notes);
        // Account and Product navigation properties are initialized but may be null until loaded
    }

    [Fact]
    public void Application_ShouldSetAndGetProperties()
    {
        // Arrange
        var application = new Application
        {
            Id = 1,
            AccountId = 1,
            ProductId = 1,
            RequestedAmount = 10000.00m,
            Status = "Approved",
            ApplicationDate = DateTime.UtcNow,
            DecisionDate = DateTime.UtcNow.AddDays(1),
            Notes = "Application approved"
        };

        // Assert
        Assert.Equal(1, application.Id);
        Assert.Equal(1, application.AccountId);
        Assert.Equal(1, application.ProductId);
        Assert.Equal(10000.00m, application.RequestedAmount);
        Assert.Equal("Approved", application.Status);
        Assert.NotNull(application.DecisionDate);
        Assert.Equal("Application approved", application.Notes);
    }

    [Fact]
    public void Application_ShouldHaveDefaultStatusAsPending()
    {
        // Act
        var application = new Application();

        // Assert
        Assert.Equal("Pending", application.Status);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    [InlineData("Completed")]
    public void Application_ShouldSupportDifferentStatuses(string status)
    {
        // Arrange
        var application = new Application
        {
            Status = status
        };

        // Assert
        Assert.Equal(status, application.Status);
    }

    [Fact]
    public void Application_CanHaveNullDecisionDate()
    {
        // Arrange
        var application = new Application
        {
            DecisionDate = null
        };

        // Assert
        Assert.Null(application.DecisionDate);
    }

    [Fact]
    public void Application_CanHaveNullNotes()
    {
        // Arrange
        var application = new Application
        {
            Notes = null
        };

        // Assert
        Assert.Null(application.Notes);
    }
}


using MyApp.Services;
using Xunit;

namespace MyApp.Tests;

public class HelloServiceTests
{
    [Fact]
    public void GetGreeting_WithDefaultName_ReturnsHelloWorld()
    {
        // Arrange
        var service = new HelloService();
        
        // Act
        var result = service.GetGreeting();
        
        // Assert
        Assert.Equal("Hello World", result);
    }
    
    [Fact]
    public void GetGreeting_WithCustomName_ReturnsHelloWithName()
    {
        // Arrange
        var service = new HelloService();
        
        // Act
        var result = service.GetGreeting("John");
        
        // Assert
        Assert.Equal("Hello John", result);
    }
    
    [Fact]
    public void GetGreeting_WithEmptyName_ReturnsHelloWorld()
    {
        // Arrange
        var service = new HelloService();
        
        // Act
        var result = service.GetGreeting("");
        
        // Assert
        Assert.Equal("Hello World", result);
    }
    
    [Fact]
    public void GetGreeting_WithNullName_ReturnsHelloWorld()
    {
        // Arrange
        var service = new HelloService();
        
        // Act
        var result = service.GetGreeting(null!);
        
        // Assert
        Assert.Equal("Hello World", result);
    }
}


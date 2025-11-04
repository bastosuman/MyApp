using MyApp.Data;
using Xunit;

namespace MyApp.Tests.DataTests;

public class DbInitializerNullSafetyTests
{
    [Fact]
    public void DbInitializer_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DbInitializer.Initialize(null!));
    }
}


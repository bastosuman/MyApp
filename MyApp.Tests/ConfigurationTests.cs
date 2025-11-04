using Microsoft.Extensions.Configuration;
using Xunit;

namespace MyApp.Tests
{
    public class ConfigurationTests
    {
        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private static string GetConnectionString()
        {
            var configuration = BuildConfiguration();
            return configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        [Fact]
        public void ConnectionString_ShouldBeConfigured()
        {
            // Act
            var connectionString = GetConnectionString();

            // Assert
            Assert.NotNull(connectionString);
            Assert.NotEmpty(connectionString);
        }

        [Theory]
        [InlineData("Server=localhost")]
        [InlineData("Database=MyAppFinancial")]
        [InlineData("Trusted_Connection=True")]
        [InlineData("TrustServerCertificate=True")]
        public void ConnectionString_ShouldContainRequiredParts(string expectedPart)
        {
            // Act
            var connectionString = GetConnectionString();

            // Assert
            Assert.Contains(expectedPart, connectionString, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Logging_ShouldBeConfigured()
        {
            // Arrange
            var configuration = BuildConfiguration();

            // Act
            var loggingSection = configuration.GetSection("Logging");
            var defaultLogLevel = loggingSection.GetSection("LogLevel:Default").Value;

            // Assert
            Assert.NotNull(loggingSection);
            Assert.NotNull(defaultLogLevel);
        }
    }
}


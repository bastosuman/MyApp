using Microsoft.Extensions.Configuration;
using Xunit;

namespace MyApp.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void ConnectionString_ShouldBeConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Act
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            Assert.NotNull(connectionString);
            Assert.NotEmpty(connectionString);
        }

        [Fact]
        public void ConnectionString_ShouldPointToLocalhost()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Act
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            Assert.Contains("Server=localhost", connectionString, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ConnectionString_ShouldContainDatabaseName()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Act
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            Assert.Contains("Database=MyAppFinancial", connectionString, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ConnectionString_ShouldUseTrustedConnection()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Act
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            Assert.Contains("Trusted_Connection=True", connectionString, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ConnectionString_ShouldContainTrustServerCertificateForLocalDevelopment()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Act
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            // TrustServerCertificate=True is required for local development with SQL Server
            // when SSL certificate validation fails. This is acceptable for local/dev environments.
            // In production, this should be removed and proper SSL certificates should be used.
            Assert.Contains("TrustServerCertificate=True", connectionString, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Logging_ShouldBeConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Act
            var loggingSection = configuration.GetSection("Logging");
            var defaultLogLevel = loggingSection.GetSection("LogLevel:Default").Value;

            // Assert
            Assert.NotNull(loggingSection);
            Assert.NotNull(defaultLogLevel);
        }
    }
}


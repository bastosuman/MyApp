using Xunit;

namespace MyApp.Tests
{
    public class ApplicationTests
    {
        [Fact]
        public void Application_ShouldHaveControllers()
        {
            // Arrange & Act
            var controllersDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp", "Controllers");
            var controllersPath = Path.GetFullPath(controllersDirectory);

            // Assert
            Assert.True(Directory.Exists(controllersPath), "Controllers directory should exist");
        }

        [Fact]
        public void AppSettings_ShouldExist()
        {
            // Arrange & Act
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp", "appsettings.json");
            var fullPath = Path.GetFullPath(appSettingsPath);

            // Assert
            Assert.True(File.Exists(fullPath), $"appsettings.json should exist at {fullPath}");
        }

        [Fact]
        public void AppSettingsDevelopment_ShouldExist()
        {
            // Arrange & Act
            var appSettingsDevPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp", "appsettings.Development.json");
            var fullPath = Path.GetFullPath(appSettingsDevPath);

            // Assert
            Assert.True(File.Exists(fullPath), $"appsettings.Development.json should exist at {fullPath}");
        }

        [Fact]
        public void Program_ShouldExist()
        {
            // Arrange & Act
            var programPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "MyApp", "Program.cs");
            var fullPath = Path.GetFullPath(programPath);

            // Assert
            Assert.True(File.Exists(fullPath), $"Program.cs should exist at {fullPath}");
        }
    }
}


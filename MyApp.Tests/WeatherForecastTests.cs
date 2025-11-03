using MyApp;
using Xunit;

namespace MyApp.Tests
{
    public class WeatherForecastTests
    {
        [Fact]
        public void TemperatureF_CalculatesCorrectly()
        {
            // Arrange
            var forecast = new WeatherForecast
            {
                TemperatureC = 0
            };

            // Act
            var temperatureF = forecast.TemperatureF;

            // Assert
            Assert.Equal(32, temperatureF);
        }

        [Fact]
        public void TemperatureF_CalculatesCorrectlyForPositiveCelsius()
        {
            // Arrange
            var forecast = new WeatherForecast
            {
                TemperatureC = 25
            };

            // Act
            var temperatureF = forecast.TemperatureF;

            // Assert
            var expected = 32 + (int)(25 / 0.5556);
            Assert.Equal(expected, temperatureF);
        }

        [Fact]
        public void TemperatureF_CalculatesCorrectlyForNegativeCelsius()
        {
            // Arrange
            var forecast = new WeatherForecast
            {
                TemperatureC = -10
            };

            // Act
            var temperatureF = forecast.TemperatureF;

            // Assert
            var expected = 32 + (int)(-10 / 0.5556);
            Assert.Equal(expected, temperatureF);
        }

        [Fact]
        public void TemperatureF_CalculatesCorrectlyForHighTemperature()
        {
            // Arrange
            var forecast = new WeatherForecast
            {
                TemperatureC = 100
            };

            // Act
            var temperatureF = forecast.TemperatureF;

            // Assert
            var expected = 32 + (int)(100 / 0.5556);
            Assert.Equal(expected, temperatureF);
        }

        [Theory]
        [InlineData(-20)]
        [InlineData(0)]
        [InlineData(25)]
        [InlineData(54)]
        public void TemperatureF_CalculatesForVariousTemperatures(int temperatureC)
        {
            // Arrange
            var forecast = new WeatherForecast
            {
                TemperatureC = temperatureC
            };

            // Act
            var temperatureF = forecast.TemperatureF;

            // Assert
            var expected = 32 + (int)(temperatureC / 0.5556);
            Assert.Equal(expected, temperatureF);
        }

        [Fact]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var forecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 20,
                Summary = "Warm"
            };

            // Assert
            Assert.NotEqual(default(DateOnly), forecast.Date);
            Assert.Equal(20, forecast.TemperatureC);
            Assert.Equal("Warm", forecast.Summary);
        }

        [Fact]
        public void Summary_CanBeNull()
        {
            // Arrange & Act
            var forecast = new WeatherForecast
            {
                Summary = null
            };

            // Assert
            Assert.Null(forecast.Summary);
        }
    }
}


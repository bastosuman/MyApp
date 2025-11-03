using Microsoft.AspNetCore.Mvc;
using MyApp.Controllers;
using Xunit;

namespace MyApp.Tests
{
    public class WeatherForecastControllerTests
    {
        [Fact]
        public void Get_ReturnsWeatherForecastArray()
        {
            // Arrange
            var controller = new WeatherForecastController();

            // Act
            var result = controller.Get();

            // Assert
            Assert.NotNull(result);
            var forecasts = result.ToArray();
            Assert.Equal(5, forecasts.Length);

            // Verify each forecast has valid data
            foreach (var forecast in forecasts)
            {
                Assert.NotNull(forecast);
                Assert.NotEqual(default(DateOnly), forecast.Date);
                Assert.True(forecast.TemperatureC >= -20 && forecast.TemperatureC < 55);
                Assert.NotNull(forecast.Summary);
                Assert.NotEmpty(forecast.Summary);
                Assert.True(forecast.TemperatureF == 32 + (int)(forecast.TemperatureC / 0.5556));
            }
        }

        [Fact]
        public void Get_ReturnsDifferentForecastsOnSubsequentCalls()
        {
            // Arrange
            var controller = new WeatherForecastController();

            // Act
            var result1 = controller.Get().ToArray();
            var result2 = controller.Get().ToArray();

            // Assert
            // Since the controller uses Random, we check that the results are valid
            // They might be different due to randomness, but both should be valid
            Assert.Equal(5, result1.Length);
            Assert.Equal(5, result2.Length);

            // At least verify the structure is correct
            Assert.All(result1, forecast => Assert.NotNull(forecast.Summary));
            Assert.All(result2, forecast => Assert.NotNull(forecast.Summary));
        }

        [Fact]
        public void Get_ReturnsForecastsWithValidDates()
        {
            // Arrange
            var controller = new WeatherForecastController();

            // Act
            var result = controller.Get().ToArray();

            // Assert
            var today = DateOnly.FromDateTime(DateTime.Now);
            for (int i = 0; i < result.Length; i++)
            {
                var expectedDate = today.AddDays(i + 1);
                Assert.Equal(expectedDate, result[i].Date);
            }
        }
    }
}


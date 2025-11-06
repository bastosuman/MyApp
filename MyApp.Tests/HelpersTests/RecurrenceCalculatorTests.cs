using MyApp.Helpers;

namespace MyApp.Tests.HelpersTests;

public class RecurrenceCalculatorTests
{
    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnScheduledDate_WhenOneTime()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow.AddDays(5);

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "OneTime", null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scheduledDate, result.Value);
    }

    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnTomorrow_WhenDaily()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow;

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "Daily", null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value > DateTime.UtcNow);
        Assert.True(result.Value <= DateTime.UtcNow.AddDays(1).AddHours(1));
    }

    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnNextWeek_WhenWeekly()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow;

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "Weekly", null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value > DateTime.UtcNow);
        Assert.True(result.Value <= DateTime.UtcNow.AddDays(8));
    }

    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnNextMonth_WhenMonthly()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow;

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "Monthly", null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value > DateTime.UtcNow);
        Assert.True(result.Value <= DateTime.UtcNow.AddMonths(1).AddDays(1));
    }

    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnNextMonthWithDay_WhenMonthlyWithDay()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow;
        var day = 15;

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "Monthly", day);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value > DateTime.UtcNow);
        Assert.Equal(15, result.Value.Day);
    }

    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnNextQuarter_WhenQuarterly()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow;

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "Quarterly", null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value > DateTime.UtcNow);
        Assert.True(result.Value <= DateTime.UtcNow.AddMonths(3).AddDays(1));
    }

    [Fact]
    public void CalculateNextExecutionDate_ShouldReturnNextYear_WhenAnnually()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow;

        // Act
        var result = RecurrenceCalculator.CalculateNextExecutionDate(scheduledDate, "Annually", null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value > DateTime.UtcNow);
        Assert.True(result.Value <= DateTime.UtcNow.AddYears(1).AddDays(1));
    }
}


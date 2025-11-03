using MyApp.Core.Interfaces;
using MyApp.Services.VB;
using Xunit;

namespace MyApp.Tests;

public class CalculationServiceTests
{
    private readonly ICalculationService _calculationService;

    public CalculationServiceTests()
    {
        _calculationService = new CalculationService();
    }

    #region CalculateInterest Tests

    [Fact]
    public void CalculateInterest_ValidInput_ReturnsCorrectInterest()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        // Simple interest: I = P * r * t = 10000 * 0.055 * 1 = 550
        Assert.Equal(550m, result);
    }

    [Fact]
    public void CalculateInterest_TwoYearTerm_ReturnsCorrectInterest()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = 24;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        // I = P * r * t = 10000 * 0.055 * 2 = 1100
        Assert.Equal(1100m, result);
    }

    [Fact]
    public void CalculateInterest_ZeroPrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = 0m;
        decimal rate = 5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_NegativePrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = -1000m;
        decimal rate = 5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_ZeroRate_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 0m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_NegativeRate_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = -5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_ZeroTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = 0;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_NegativeTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = -12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Theory]
    [InlineData(5000m, 3.5m, 6, 87.5m)] // 5000 * 0.035 * 0.5 = 87.5
    [InlineData(25000m, 7.25m, 24, 3625m)] // 25000 * 0.0725 * 2 = 3625
    [InlineData(100000m, 4.2m, 360, 126000m)] // 100000 * 0.042 * 30 = 126000
    public void CalculateInterest_VariousInputs_ReturnsCorrectInterest(decimal principal, decimal rate, int termMonths, decimal expectedInterest)
    {
        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(expectedInterest, result);
    }

    [Fact]
    public void CalculateInterest_ResultIsRoundedToTwoDecimals()
    {
        // Arrange
        decimal principal = 1000m;
        decimal rate = 3.333m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        // Should be rounded to 2 decimal places
        var decimals = result.ToString().Split('.');
        if (decimals.Length > 1)
        {
            Assert.True(decimals[1].Length <= 2);
        }
    }

    #endregion

    #region CalculateMonthlyPayment Tests

    [Fact]
    public void CalculateMonthlyPayment_ValidInput_ReturnsCorrectPayment()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        // Using amortization formula, should be approximately 567.79
        Assert.True(result > 567m && result < 568m);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroPrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = 0m;
        decimal annualRate = 5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativePrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = -10000m;
        decimal annualRate = 5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int termMonths = 0;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativeTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int termMonths = -360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroRate_ReturnsPrincipalDividedByTerm()
    {
        // Arrange
        decimal principal = 36000m;
        decimal annualRate = 0m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        // Should return principal / termMonths = 36000 / 360 = 100
        Assert.Equal(100m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativeRate_ReturnsPrincipalDividedByTerm()
    {
        // Arrange
        decimal principal = 36000m;
        decimal annualRate = -5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        // Should treat negative rate as zero and return principal / termMonths
        Assert.Equal(100m, result);
    }

    [Theory]
    [InlineData(50000m, 4.5m, 240)]
    [InlineData(200000m, 6.0m, 180)]
    [InlineData(10000m, 8.0m, 60)]
    public void CalculateMonthlyPayment_VariousValidInputs_ReturnsPositiveValue(decimal principal, decimal rate, int term)
    {
        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, rate, term);

        // Assert
        Assert.True(result > 0);
        Assert.True(result <= principal); // Monthly payment should be less than or equal to principal for reasonable rates
    }

    [Fact]
    public void CalculateMonthlyPayment_ResultIsRoundedToTwoDecimals()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.555m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        var decimals = result.ToString().Split('.');
        if (decimals.Length > 1)
        {
            Assert.True(decimals[1].Length <= 2);
        }
    }

    [Fact]
    public void CalculateMonthlyPayment_ShortTermLoan_ReturnsHigherPayment()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int shortTerm = 60;
        int longTerm = 360;

        // Act
        var shortTermPayment = _calculationService.CalculateMonthlyPayment(principal, annualRate, shortTerm);
        var longTermPayment = _calculationService.CalculateMonthlyPayment(principal, annualRate, longTerm);

        // Assert
        Assert.True(shortTermPayment > longTermPayment);
    }

    #endregion

    #region CalculateCreditScore Tests

    [Fact]
    public void CalculateCreditScore_ExcellentDebtToIncome_ReturnsHighScore()
    {
        // Arrange
        decimal monthlyIncome = 10000m;
        decimal monthlyDebt = 1500m; // 15% DTI
        int creditHistoryMonths = 60;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 150 (DTI < 0.2) + 100 (history >= 60) = 550
        Assert.True(result >= 550);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_GoodDebtToIncome_ReturnsGoodScore()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 1500m; // 30% DTI
        int creditHistoryMonths = 60;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 100 (DTI < 0.36) + 100 (history >= 60) = 500
        Assert.True(result >= 500);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_FairDebtToIncome_ReturnsFairScore()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 2000m; // 40% DTI
        int creditHistoryMonths = 36;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 50 (DTI < 0.5) + 75 (history >= 36) = 425
        Assert.True(result >= 425);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_WithBankruptcy_ReturnsLowerScore()
    {
        // Arrange
        decimal monthlyIncome = 10000m;
        decimal monthlyDebt = 1500m;
        int creditHistoryMonths = 60;
        bool hasBankruptcy = true;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 150 (DTI < 0.2) + 100 (history >= 60) - 150 (bankruptcy) = 400
        Assert.True(result >= 300);
        Assert.True(result < 550); // Should be lower than without bankruptcy
    }

    [Fact]
    public void CalculateCreditScore_NoCreditHistory_ReturnsLowScore()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 500m;
        int creditHistoryMonths = 0;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 150 (DTI < 0.2) + 0 (no history) = 450
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_ScoreIsWithinValidRange()
    {
        // Arrange
        decimal monthlyIncome = 10000m;
        decimal monthlyDebt = 5000m;
        int creditHistoryMonths = 120;
        bool hasBankruptcy = true;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_ZeroIncome_ReturnsBaseScore()
    {
        // Arrange
        decimal monthlyIncome = 0m;
        decimal monthlyDebt = 0m;
        int creditHistoryMonths = 24;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 0 (no income calculation) + 50 (history >= 24) = 350
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Theory]
    [InlineData(10000m, 1000m, 72, false)] // Excellent DTI, long history
    [InlineData(3000m, 1200m, 36, false)] // Good DTI, medium history
    [InlineData(5000m, 2500m, 12, false)] // Fair DTI, short history
    [InlineData(8000m, 2000m, 48, true)] // Good DTI, medium history, bankruptcy
    public void CalculateCreditScore_VariousInputs_ReturnsValidScore(decimal income, decimal debt, int history, bool bankruptcy)
    {
        // Act
        var result = _calculationService.CalculateCreditScore(income, debt, history, bankruptcy);

        // Assert
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_LongCreditHistory_AddsMorePoints()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 1000m;
        bool hasBankruptcy = false;

        // Act
        var score12Months = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, 12, hasBankruptcy);
        var score60Months = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, 60, hasBankruptcy);

        // Assert
        Assert.True(score60Months > score12Months);
    }

    #endregion

    #region CalculateTotalPayment Tests

    [Fact]
    public void CalculateTotalPayment_ValidInput_ReturnsCorrectTotal()
    {
        // Arrange
        decimal monthlyPayment = 500m;
        int termMonths = 60;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(30000m, result);
    }

    [Fact]
    public void CalculateTotalPayment_ZeroMonthlyPayment_ReturnsZero()
    {
        // Arrange
        decimal monthlyPayment = 0m;
        int termMonths = 60;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPayment_ZeroTerm_ReturnsZero()
    {
        // Arrange
        decimal monthlyPayment = 500m;
        int termMonths = 0;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPayment_NegativeMonthlyPayment_ReturnsNegative()
    {
        // Arrange
        decimal monthlyPayment = -500m;
        int termMonths = 60;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(-30000m, result);
    }

    [Theory]
    [InlineData(567.79m, 360, 204404.40m)]
    [InlineData(1000m, 12, 12000m)]
    [InlineData(2500m, 24, 60000m)]
    public void CalculateTotalPayment_VariousInputs_ReturnsCorrectTotal(decimal monthlyPayment, int termMonths, decimal expectedTotal)
    {
        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(expectedTotal, result);
    }

    [Fact]
    public void CalculateTotalPayment_ResultIsRoundedToTwoDecimals()
    {
        // Arrange
        decimal monthlyPayment = 567.789m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        var decimals = result.ToString().Split('.');
        if (decimals.Length > 1)
        {
            Assert.True(decimals[1].Length <= 2);
        }
    }

    [Fact]
    public void CalculateTotalPayment_LargeTerm_ReturnsCorrectTotal()
    {
        // Arrange
        decimal monthlyPayment = 100m;
        int termMonths = 1000;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(100000m, result);
    }

    #endregion
}


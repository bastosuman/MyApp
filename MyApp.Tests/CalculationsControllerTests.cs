using Microsoft.AspNetCore.Mvc;
using Moq;
using MyApp.Controllers;
using MyApp.Core.Interfaces;
using Xunit;

namespace MyApp.Tests;

public class CalculationsControllerTests
{
    private readonly Mock<ICalculationService> _mockCalculationService;
    private readonly CalculationsController _controller;

    public CalculationsControllerTests()
    {
        _mockCalculationService = new Mock<ICalculationService>();
        _controller = new CalculationsController(_mockCalculationService.Object);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        var mockService = new Mock<ICalculationService>();

        // Act
        var controller = new CalculationsController(mockService.Object);

        // Assert
        Assert.NotNull(controller);
    }

    #region CalculateInterest Tests

    [Fact]
    public void CalculateInterest_ValidRequest_ReturnsOkWithInterest()
    {
        // Arrange
        var request = new InterestCalculationRequest(10000m, 5.5m, 12);
        var expectedInterest = 550m;
        _mockCalculationService.Setup(x => x.CalculateInterest(10000m, 5.5m, 12))
            .Returns(expectedInterest);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<InterestResponse>(okResult.Value);
        Assert.Equal(expectedInterest, response.Interest);
        _mockCalculationService.Verify(x => x.CalculateInterest(10000m, 5.5m, 12), Times.Once);
    }

    [Fact]
    public void CalculateInterest_ZeroPrincipal_ReturnsBadRequest()
    {
        // Arrange
        var request = new InterestCalculationRequest(0m, 5.5m, 12);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
        _mockCalculationService.Verify(x => x.CalculateInterest(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CalculateInterest_NegativePrincipal_ReturnsBadRequest()
    {
        // Arrange
        var request = new InterestCalculationRequest(-1000m, 5.5m, 12);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateInterest_ZeroRate_ReturnsBadRequest()
    {
        // Arrange
        var request = new InterestCalculationRequest(10000m, 0m, 12);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateInterest_NegativeRate_ReturnsBadRequest()
    {
        // Arrange
        var request = new InterestCalculationRequest(10000m, -5.5m, 12);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateInterest_ZeroTerm_ReturnsBadRequest()
    {
        // Arrange
        var request = new InterestCalculationRequest(10000m, 5.5m, 0);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateInterest_NegativeTerm_ReturnsBadRequest()
    {
        // Arrange
        var request = new InterestCalculationRequest(10000m, 5.5m, -12);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateInterest_MultipleInvalidFields_ReturnsBadRequest()
    {
        // Arrange - Test case where multiple validation conditions are true
        var request = new InterestCalculationRequest(0m, 0m, 0);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal, rate, and term must be greater than 0", badRequestResult.Value);
    }

    [Theory]
    [InlineData(1000.0, 3.5, 6)]
    [InlineData(50000.0, 7.25, 24)]
    [InlineData(250000.0, 4.2, 360)]
    public void CalculateInterest_VariousValidInputs_ReturnsOk(double principal, double rate, int term)
    {
        // Arrange
        var request = new InterestCalculationRequest((decimal)principal, (decimal)rate, term);
        var expectedInterest = (decimal)principal * ((decimal)rate / 100) * (term / 12m);
        _mockCalculationService.Setup(x => x.CalculateInterest((decimal)principal, (decimal)rate, term))
            .Returns(expectedInterest);

        // Act
        var result = _controller.CalculateInterest(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockCalculationService.Verify(x => x.CalculateInterest((decimal)principal, (decimal)rate, term), Times.Once);
    }

    #endregion

    #region CalculateMonthlyPayment Tests

    [Fact]
    public void CalculateMonthlyPayment_ValidRequest_ReturnsOkWithPaymentDetails()
    {
        // Arrange
        var request = new PaymentCalculationRequest(100000m, 5.5m, 360);
        var expectedMonthlyPayment = 567.79m;
        var expectedTotalPayment = 204404.40m;
        var expectedTotalInterest = 104404.40m;

        _mockCalculationService.Setup(x => x.CalculateMonthlyPayment(100000m, 5.5m, 360))
            .Returns(expectedMonthlyPayment);
        _mockCalculationService.Setup(x => x.CalculateTotalPayment(expectedMonthlyPayment, 360))
            .Returns(expectedTotalPayment);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MonthlyPaymentResponse>(okResult.Value);
        Assert.Equal(expectedMonthlyPayment, response.MonthlyPayment);
        Assert.Equal(expectedTotalPayment, response.TotalPayment);
        Assert.Equal(expectedTotalInterest, response.TotalInterest);
        _mockCalculationService.Verify(x => x.CalculateMonthlyPayment(100000m, 5.5m, 360), Times.Once);
        _mockCalculationService.Verify(x => x.CalculateTotalPayment(expectedMonthlyPayment, 360), Times.Once);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroPrincipal_ReturnsBadRequest()
    {
        // Arrange
        var request = new PaymentCalculationRequest(0m, 5.5m, 360);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal and term must be greater than 0", badRequestResult.Value);
        _mockCalculationService.Verify(x => x.CalculateMonthlyPayment(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativePrincipal_ReturnsBadRequest()
    {
        // Arrange
        var request = new PaymentCalculationRequest(-10000m, 5.5m, 360);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroTerm_ReturnsBadRequest()
    {
        // Arrange
        var request = new PaymentCalculationRequest(100000m, 5.5m, 0);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativeTerm_ReturnsBadRequest()
    {
        // Arrange
        var request = new PaymentCalculationRequest(100000m, 5.5m, -360);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal and term must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public void CalculateMonthlyPayment_BothPrincipalAndTermInvalid_ReturnsBadRequest()
    {
        // Arrange - Test case where both validation conditions are true
        var request = new PaymentCalculationRequest(0m, 5.5m, 0);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Principal and term must be greater than 0", badRequestResult.Value);
    }

    [Theory]
    [InlineData(50000.0, 4.5, 240)]
    [InlineData(200000.0, 6.0, 180)]
    [InlineData(10000.0, 8.0, 60)]
    public void CalculateMonthlyPayment_VariousValidInputs_ReturnsOk(double principal, double rate, int term)
    {
        // Arrange
        var request = new PaymentCalculationRequest((decimal)principal, (decimal)rate, term);
        var expectedMonthlyPayment = 500m;
        var expectedTotalPayment = 120000m;

        _mockCalculationService.Setup(x => x.CalculateMonthlyPayment((decimal)principal, (decimal)rate, term))
            .Returns(expectedMonthlyPayment);
        _mockCalculationService.Setup(x => x.CalculateTotalPayment(expectedMonthlyPayment, term))
            .Returns(expectedTotalPayment);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockCalculationService.Verify(x => x.CalculateMonthlyPayment((decimal)principal, (decimal)rate, term), Times.Once);
        _mockCalculationService.Verify(x => x.CalculateTotalPayment(expectedMonthlyPayment, term), Times.Once);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroRate_ReturnsOk()
    {
        // Arrange
        var request = new PaymentCalculationRequest(100000m, 0m, 360);
        var expectedMonthlyPayment = 277.78m;
        var expectedTotalPayment = 100000m;

        _mockCalculationService.Setup(x => x.CalculateMonthlyPayment(100000m, 0m, 360))
            .Returns(expectedMonthlyPayment);
        _mockCalculationService.Setup(x => x.CalculateTotalPayment(expectedMonthlyPayment, 360))
            .Returns(expectedTotalPayment);

        // Act
        var result = _controller.CalculateMonthlyPayment(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    #endregion

    #region CalculateCreditScore Tests

    [Fact]
    public void CalculateCreditScore_ValidRequest_ReturnsOkWithScore()
    {
        // Arrange
        var request = new CreditScoreRequest(5000m, 1500m, 60, false);
        var expectedScore = 650;
        _mockCalculationService.Setup(x => x.CalculateCreditScore(5000m, 1500m, 60, false))
            .Returns(expectedScore);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CreditScoreResponse>(okResult.Value);
        Assert.Equal(expectedScore, response.CreditScore);
        _mockCalculationService.Verify(x => x.CalculateCreditScore(5000m, 1500m, 60, false), Times.Once);
    }

    [Fact]
    public void CalculateCreditScore_NegativeIncome_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreditScoreRequest(-1000m, 500m, 24, false);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Income and debt cannot be negative", badRequestResult.Value);
        _mockCalculationService.Verify(x => x.CalculateCreditScore(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void CalculateCreditScore_NegativeDebt_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreditScoreRequest(5000m, -500m, 24, false);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Income and debt cannot be negative", badRequestResult.Value);
    }

    [Fact]
    public void CalculateCreditScore_BothIncomeAndDebtNegative_ReturnsBadRequest()
    {
        // Arrange - Test case where both validation conditions are true
        var request = new CreditScoreRequest(-1000m, -500m, 24, false);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Income and debt cannot be negative", badRequestResult.Value);
    }

    [Fact]
    public void CalculateCreditScore_ZeroIncome_ReturnsOk()
    {
        // Arrange
        var request = new CreditScoreRequest(0m, 0m, 12, false);
        var expectedScore = 400;
        _mockCalculationService.Setup(x => x.CalculateCreditScore(0m, 0m, 12, false))
            .Returns(expectedScore);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void CalculateCreditScore_ZeroDebt_ReturnsOk()
    {
        // Arrange
        var request = new CreditScoreRequest(5000m, 0m, 24, false);
        var expectedScore = 450;
        _mockCalculationService.Setup(x => x.CalculateCreditScore(5000m, 0m, 24, false))
            .Returns(expectedScore);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Theory]
    [InlineData(10000.0, 2000.0, 72, false)]
    [InlineData(3000.0, 1000.0, 36, true)]
    [InlineData(7500.0, 2500.0, 48, false)]
    public void CalculateCreditScore_VariousValidInputs_ReturnsOk(double income, double debt, int history, bool bankruptcy)
    {
        // Arrange
        var request = new CreditScoreRequest((decimal)income, (decimal)debt, history, bankruptcy);
        var expectedScore = 600;
        _mockCalculationService.Setup(x => x.CalculateCreditScore((decimal)income, (decimal)debt, history, bankruptcy))
            .Returns(expectedScore);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockCalculationService.Verify(x => x.CalculateCreditScore((decimal)income, (decimal)debt, history, bankruptcy), Times.Once);
    }

    [Fact]
    public void CalculateCreditScore_WithBankruptcy_ReturnsOk()
    {
        // Arrange
        var request = new CreditScoreRequest(5000m, 1500m, 60, true);
        var expectedScore = 500;
        _mockCalculationService.Setup(x => x.CalculateCreditScore(5000m, 1500m, 60, true))
            .Returns(expectedScore);

        // Act
        var result = _controller.CalculateCreditScore(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockCalculationService.Verify(x => x.CalculateCreditScore(5000m, 1500m, 60, true), Times.Once);
    }

    #endregion
}


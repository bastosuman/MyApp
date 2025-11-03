using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Interfaces;

namespace MyApp.Controllers;

/// <summary>
/// API controller for financial calculations
/// Demonstrates using VB.NET services from C# controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CalculationsController : ControllerBase
{
    private readonly ICalculationService _calculationService;

    public CalculationsController(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    /// <summary>
    /// Calculate interest for a loan
    /// </summary>
    [HttpPost("interest")]
    public IActionResult CalculateInterest([FromBody] InterestCalculationRequest request)
    {
        if (request.Principal <= 0 || request.Rate <= 0 || request.TermMonths <= 0)
        {
            return BadRequest("Principal, rate, and term must be greater than 0");
        }

        var interest = _calculationService.CalculateInterest(request.Principal, request.Rate, request.TermMonths);
        return Ok(new { Interest = interest });
    }

    /// <summary>
    /// Calculate monthly payment for a loan
    /// </summary>
    [HttpPost("monthly-payment")]
    public IActionResult CalculateMonthlyPayment([FromBody] PaymentCalculationRequest request)
    {
        if (request.Principal <= 0 || request.TermMonths <= 0)
        {
            return BadRequest("Principal and term must be greater than 0");
        }

        var monthlyPayment = _calculationService.CalculateMonthlyPayment(
            request.Principal,
            request.AnnualRate,
            request.TermMonths);
        
        var totalPayment = _calculationService.CalculateTotalPayment(monthlyPayment, request.TermMonths);
        var totalInterest = totalPayment - request.Principal;

        return Ok(new
        {
            MonthlyPayment = monthlyPayment,
            TotalPayment = totalPayment,
            TotalInterest = totalInterest
        });
    }

    /// <summary>
    /// Calculate credit score based on financial factors
    /// </summary>
    [HttpPost("credit-score")]
    public IActionResult CalculateCreditScore([FromBody] CreditScoreRequest request)
    {
        if (request.MonthlyIncome < 0 || request.MonthlyDebt < 0)
        {
            return BadRequest("Income and debt cannot be negative");
        }

        var creditScore = _calculationService.CalculateCreditScore(
            request.MonthlyIncome,
            request.MonthlyDebt,
            request.CreditHistoryMonths,
            request.HasBankruptcy);

        return Ok(new { CreditScore = creditScore });
    }
}

// Request DTOs
public record InterestCalculationRequest(decimal Principal, decimal Rate, int TermMonths);
public record PaymentCalculationRequest(decimal Principal, decimal AnnualRate, int TermMonths);
public record CreditScoreRequest(decimal MonthlyIncome, decimal MonthlyDebt, int CreditHistoryMonths, bool HasBankruptcy);


using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

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
    [ProducesResponseType(typeof(InterestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CalculateInterest([FromBody] InterestCalculationRequest request)
    {
        if (request.Principal <= 0 || request.Rate <= 0 || request.TermMonths <= 0)
        {
            return BadRequest("Principal, rate, and term must be greater than 0");
        }

        var interest = _calculationService.CalculateInterest(request.Principal, request.Rate, request.TermMonths);
        return Ok(new InterestResponse { Interest = interest });
    }

    /// <summary>
    /// Calculate monthly payment for a loan
    /// </summary>
    [HttpPost("monthly-payment")]
    [ProducesResponseType(typeof(MonthlyPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        return Ok(new MonthlyPaymentResponse
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
    [ProducesResponseType(typeof(CreditScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        return Ok(new CreditScoreResponse { CreditScore = creditScore });
    }
}

// Request DTOs
public record InterestCalculationRequest(
    [Required] decimal Principal, 
    [Required] decimal Rate, 
    [Required] int TermMonths);

public record PaymentCalculationRequest(
    [Required] decimal Principal, 
    [Required] decimal AnnualRate, 
    [Required] int TermMonths);

public record CreditScoreRequest(
    [Required] decimal MonthlyIncome, 
    [Required] decimal MonthlyDebt, 
    [Required] int CreditHistoryMonths, 
    [Required] bool HasBankruptcy);

// Response DTOs
public record InterestResponse(decimal Interest);
public record MonthlyPaymentResponse(decimal MonthlyPayment, decimal TotalPayment, decimal TotalInterest);
public record CreditScoreResponse(int CreditScore);


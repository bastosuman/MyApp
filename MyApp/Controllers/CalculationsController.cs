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
        if (request.Principal == null || request.Rate == null || request.TermMonths == null ||
            request.Principal <= 0 || request.Rate <= 0 || request.TermMonths <= 0)
        {
            return BadRequest("Principal, rate, and term must be greater than 0");
        }

        var interest = _calculationService.CalculateInterest(request.Principal.Value, request.Rate.Value, request.TermMonths.Value);
        return Ok(new InterestResponse(interest));
    }

    /// <summary>
    /// Calculate monthly payment for a loan
    /// </summary>
    [HttpPost("monthly-payment")]
    [ProducesResponseType(typeof(MonthlyPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CalculateMonthlyPayment([FromBody] PaymentCalculationRequest request)
    {
        if (request.Principal == null || request.TermMonths == null ||
            request.Principal <= 0 || request.TermMonths <= 0)
        {
            return BadRequest("Principal and term must be greater than 0");
        }

        var monthlyPayment = _calculationService.CalculateMonthlyPayment(
            request.Principal.Value,
            request.AnnualRate ?? 0m,
            request.TermMonths.Value);
        
        var totalPayment = _calculationService.CalculateTotalPayment(monthlyPayment, request.TermMonths.Value);
        var totalInterest = totalPayment - request.Principal.Value;

        return Ok(new MonthlyPaymentResponse(monthlyPayment, totalPayment, totalInterest));
    }

    /// <summary>
    /// Calculate credit score based on financial factors
    /// </summary>
    [HttpPost("credit-score")]
    [ProducesResponseType(typeof(CreditScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CalculateCreditScore([FromBody] CreditScoreRequest request)
    {
        if (request.MonthlyIncome == null || request.MonthlyDebt == null ||
            request.MonthlyIncome < 0 || request.MonthlyDebt < 0)
        {
            return BadRequest("Income and debt cannot be negative");
        }

        var creditScore = _calculationService.CalculateCreditScore(
            request.MonthlyIncome.Value,
            request.MonthlyDebt.Value,
            request.CreditHistoryMonths ?? 0,
            request.HasBankruptcy ?? false);

        return Ok(new CreditScoreResponse(creditScore));
    }
}

// Request DTOs
public record InterestCalculationRequest(
    [Required] decimal? Principal, 
    [Required] decimal? Rate, 
    [Required] int? TermMonths);

public record PaymentCalculationRequest(
    [Required] decimal? Principal, 
    [Required] decimal? AnnualRate, 
    [Required] int? TermMonths);

public record CreditScoreRequest(
    [Required] decimal? MonthlyIncome, 
    [Required] decimal? MonthlyDebt, 
    [Required] int? CreditHistoryMonths, 
    [Required] bool? HasBankruptcy);

// Response DTOs
public record InterestResponse(decimal Interest);
public record MonthlyPaymentResponse(decimal MonthlyPayment, decimal TotalPayment, decimal TotalInterest);
public record CreditScoreResponse(int CreditScore);


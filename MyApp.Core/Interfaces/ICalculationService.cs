namespace MyApp.Core.Interfaces;

/// <summary>
/// Interface for financial calculation services (to be implemented in VB.NET)
/// </summary>
public interface ICalculationService
{
    /// <summary>
    /// Calculates interest for a loan
    /// </summary>
    decimal CalculateInterest(decimal principal, decimal rate, int termMonths);
    
    /// <summary>
    /// Calculates monthly payment using amortization formula
    /// </summary>
    decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths);
    
    /// <summary>
    /// Calculates credit score based on various factors
    /// </summary>
    int CalculateCreditScore(decimal monthlyIncome, decimal monthlyDebt, int creditHistoryMonths, bool hasBankruptcy);
    
    /// <summary>
    /// Calculates total amount paid over loan term
    /// </summary>
    decimal CalculateTotalPayment(decimal monthlyPayment, int termMonths);
}



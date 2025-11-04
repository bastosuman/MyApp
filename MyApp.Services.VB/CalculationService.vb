Imports System
Imports MyApp.Core.Interfaces

Namespace MyApp.Services.VB

    ''' <summary>
    ''' VB.NET implementation of financial calculation services
    ''' This demonstrates interoperability between C# and VB.NET
    ''' </summary>
    Public Class CalculationService
        Implements ICalculationService

        ''' <summary>
        ''' Calculates interest for a loan using simple interest formula
        ''' Formula: I = P * r * t
        ''' </summary>
        Public Function CalculateInterest(principal As Decimal, rate As Decimal, termMonths As Integer) As Decimal Implements ICalculationService.CalculateInterest
            If principal <= 0 Then
                Return 0
            End If
            
            If rate <= 0 OrElse termMonths <= 0 Then
                Return 0
            End If
            
            ' Convert term to years (explicit conversion for Option Strict)
            Dim years As Decimal = CDec(termMonths) / 12D
            
            ' Simple interest calculation
            Dim interest As Decimal = principal * (rate / 100D) * years
            
            Return Math.Round(interest, 2)
        End Function

        ''' <summary>
        ''' Calculates monthly payment using amortization formula
        ''' Formula: M = P * [r(1+r)^n] / [(1+r)^n - 1]
        ''' </summary>
        Public Function CalculateMonthlyPayment(principal As Decimal, annualRate As Decimal, termMonths As Integer) As Decimal Implements ICalculationService.CalculateMonthlyPayment
            If principal <= 0 OrElse termMonths <= 0 Then
                Return 0
            End If
            
            If annualRate <= 0 Then
                Return principal / termMonths
            End If
            
            ' Convert annual rate to monthly
            Dim monthlyRateValue As Decimal = annualRate / 100D / 12D
            
            ' Calculate (1+r)^n
            Dim onePlusRToN As Decimal = CDec(Math.Pow(CDbl(1 + monthlyRateValue), termMonths))
            
            ' Amortization formula
            Dim monthlyPayment As Decimal = principal * (monthlyRateValue * onePlusRToN) / (onePlusRToN - 1)
            
            Return Math.Round(monthlyPayment, 2)
        End Function

        ''' <summary>
        ''' Calculates credit score based on financial factors
        ''' Simplified scoring model for demonstration
        ''' </summary>
        Public Function CalculateCreditScore(monthlyIncome As Decimal, monthlyDebt As Decimal, creditHistoryMonths As Integer, hasBankruptcy As Boolean) As Integer Implements ICalculationService.CalculateCreditScore
            Dim score As Integer = 300 ' Base score
            
            ' Income to debt ratio (ideal is < 0.36)
            If monthlyIncome > 0 Then
                Dim debtToIncomeRatio As Decimal = monthlyDebt / monthlyIncome
                
                If debtToIncomeRatio < 0.2 Then
                    score += 150
                ElseIf debtToIncomeRatio < 0.36 Then
                    score += 100
                ElseIf debtToIncomeRatio < 0.5 Then
                    score += 50
                End If
            End If
            
            ' Credit history
            If creditHistoryMonths >= 60 Then
                score += 100
            ElseIf creditHistoryMonths >= 36 Then
                score += 75
            ElseIf creditHistoryMonths >= 24 Then
                score += 50
            ElseIf creditHistoryMonths >= 12 Then
                score += 25
            End If
            
            ' Bankruptcy check
            If hasBankruptcy Then
                score -= 150
            End If
            
            ' Ensure score is within valid range (300-850)
            If score < 300 Then
                score = 300
            End If
            
            ' Cap maximum score at 850
            If score > 850 Then
                score = 850
            End If
            
            Return score
        End Function

        ''' <summary>
        ''' Calculates total amount paid over loan term
        ''' </summary>
        Public Function CalculateTotalPayment(monthlyPayment As Decimal, termMonths As Integer) As Decimal Implements ICalculationService.CalculateTotalPayment
            Return Math.Round(monthlyPayment * termMonths, 2)
        End Function
    End Class

End Namespace



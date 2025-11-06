using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in account validation logic
/// </summary>
public static class AccountValidator
{
    /// <summary>
    /// Validates an account and returns error response if invalid
    /// </summary>
    private static ActionResult<ApiResponse<T>>? ValidateAccount<T>(Account? account, string accountType)
    {
        if (account == null || !account.IsActive)
        {
            return ControllerErrorHandler.BadRequestResponse<T>($"{accountType} account not found or inactive");
        }
        return null;
    }

    /// <summary>
    /// Validates source account and returns error response if invalid
    /// </summary>
    public static async Task<ActionResult<ApiResponse<T>>?> ValidateSourceAccountAsync<T>(
        FinancialDbContext context,
        int accountId)
    {
        var account = await context.Accounts.FindAsync(accountId);
        return ValidateAccount<T>(account, "Source");
    }

    /// <summary>
    /// Validates destination account by ID and returns error response if invalid
    /// </summary>
    public static async Task<ActionResult<ApiResponse<T>>?> ValidateDestinationAccountByIdAsync<T>(
        FinancialDbContext context,
        int accountId)
    {
        var account = await context.Accounts.FindAsync(accountId);
        return ValidateAccount<T>(account, "Destination");
    }

    /// <summary>
    /// Validates destination account by account number and returns error response if invalid
    /// </summary>
    public static async Task<(ActionResult<ApiResponse<T>>? Error, Account? Account)> ValidateDestinationAccountByNumberAsync<T>(
        FinancialDbContext context,
        string accountNumber)
    {
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        var error = ValidateAccount<T>(account, "Destination");
        return error != null ? (error, null) : (null, account);
    }
}


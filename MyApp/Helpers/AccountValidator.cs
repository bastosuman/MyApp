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
    /// Validates source account and returns error response if invalid
    /// </summary>
    public static async Task<ActionResult<ApiResponse<T>>?> ValidateSourceAccountAsync<T>(
        FinancialDbContext context,
        int accountId)
    {
        var account = await context.Accounts.FindAsync(accountId);
        if (account == null || !account.IsActive)
        {
            return ControllerErrorHandler.BadRequestResponse<T>("Source account not found or inactive");
        }
        return null;
    }

    /// <summary>
    /// Validates destination account by ID and returns error response if invalid
    /// </summary>
    public static async Task<ActionResult<ApiResponse<T>>?> ValidateDestinationAccountByIdAsync<T>(
        FinancialDbContext context,
        int accountId)
    {
        var account = await context.Accounts.FindAsync(accountId);
        if (account == null || !account.IsActive)
        {
            return ControllerErrorHandler.BadRequestResponse<T>("Destination account not found or inactive");
        }
        return null;
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
        if (account == null || !account.IsActive)
        {
            return (ControllerErrorHandler.BadRequestResponse<T>("Destination account not found or inactive"), null);
        }
        return (null, account);
    }
}


using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;

namespace MyApp.Services;

/// <summary>
/// Helper to reduce cognitive complexity in transfer validation
/// </summary>
internal static class TransferValidationHelper
{
    /// <summary>
    /// Validates source account
    /// </summary>
    public static (bool IsValid, string? ErrorMessage, Account? Account) ValidateSourceAccount(
        Account? sourceAccount)
    {
        if (sourceAccount == null)
        {
            return (false, "Source account not found", null);
        }

        if (!sourceAccount.IsActive)
        {
            return (false, "Source account is not active", null);
        }

        return (true, null, sourceAccount);
    }

    /// <summary>
    /// Validates transfer amount
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            return (false, "Transfer amount must be greater than zero");
        }
        return (true, null);
    }

    /// <summary>
    /// Validates destination account for internal transfer
    /// </summary>
    public static (bool IsValid, string? ErrorMessage, Account? Account) ValidateInternalDestination(
        Account? destinationAccount,
        int sourceAccountId,
        int destinationAccountId)
    {
        if (destinationAccount == null)
        {
            return (false, "Destination account not found", null);
        }

        if (!destinationAccount.IsActive)
        {
            return (false, "Destination account is not active", null);
        }

        if (sourceAccountId == destinationAccountId)
        {
            return (false, "Cannot transfer to the same account", null);
        }

        return (true, null, destinationAccount);
    }

    /// <summary>
    /// Validates destination account for external transfer
    /// </summary>
    public static (bool IsValid, string? ErrorMessage, Account? Account) ValidateExternalDestination(
        Account? destinationAccount,
        string sourceAccountNumber,
        string destinationAccountNumber)
    {
        if (destinationAccount == null)
        {
            return (false, "Destination account not found", null);
        }

        if (!destinationAccount.IsActive)
        {
            return (false, "Destination account is not active", null);
        }

        if (sourceAccountNumber == destinationAccountNumber)
        {
            return (false, "Cannot transfer to the same account", null);
        }

        return (true, null, destinationAccount);
    }

    /// <summary>
    /// Validates account balance
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateBalance(Account sourceAccount, decimal amount)
    {
        if (sourceAccount.Balance < amount)
        {
            return (false, "Insufficient balance for transfer");
        }
        return (true, null);
    }
}


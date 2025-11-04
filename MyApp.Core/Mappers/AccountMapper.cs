using MyApp.Core.DTOs;
using MyApp.Core.Entities;

namespace MyApp.Core.Mappers;

public static class AccountMapper
{
    /// <summary>
    /// Maps Account entity to AccountDto
    /// </summary>
    public static AccountDto ToDto(this Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountHolderName = account.AccountHolderName,
            Balance = account.Balance,
            AccountType = account.AccountType,
            CreatedDate = account.CreatedDate,
            IsActive = account.IsActive
        };
    }
}


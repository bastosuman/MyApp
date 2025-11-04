using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Core.Mappers;

namespace MyApp.Tests.MappersTests;

public class AccountMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAccountToDto()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            AccountNumber = "ACC001",
            AccountHolderName = "John Doe",
            Balance = 1000.50m,
            AccountType = "Savings",
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var dto = account.ToDto();

        // Assert
        Assert.Equal(account.Id, dto.Id);
        Assert.Equal(account.AccountNumber, dto.AccountNumber);
        Assert.Equal(account.AccountHolderName, dto.AccountHolderName);
        Assert.Equal(account.Balance, dto.Balance);
        Assert.Equal(account.AccountType, dto.AccountType);
        Assert.Equal(account.CreatedDate, dto.CreatedDate);
        Assert.Equal(account.IsActive, dto.IsActive);
    }
}


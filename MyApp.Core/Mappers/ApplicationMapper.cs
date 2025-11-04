using MyApp.Core.DTOs;
using MyApp.Core.Entities;

namespace MyApp.Core.Mappers;

public static class ApplicationMapper
{
    /// <summary>
    /// Maps Application entity to ApplicationDto
    /// </summary>
    public static ApplicationDto ToDto(this Application application)
    {
        return new ApplicationDto
        {
            Id = application.Id,
            AccountId = application.AccountId,
            ProductId = application.ProductId,
            RequestedAmount = application.RequestedAmount,
            Status = application.Status,
            ApplicationDate = application.ApplicationDate,
            DecisionDate = application.DecisionDate,
            Notes = application.Notes,
            AccountNumber = application.Account?.AccountNumber,
            ProductName = application.Product?.Name
        };
    }
}


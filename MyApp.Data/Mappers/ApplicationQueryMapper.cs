using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;

namespace MyApp.Data.Mappers;

public static class ApplicationQueryMapper
{
    /// <summary>
    /// Maps Application query to ApplicationDto projection for EF Core queries
    /// </summary>
    public static IQueryable<ApplicationDto> ToDtoQuery(this IQueryable<Application> query)
    {
        return query.Select(a => new ApplicationDto
        {
            Id = a.Id,
            AccountId = a.AccountId,
            ProductId = a.ProductId,
            RequestedAmount = a.RequestedAmount,
            Status = a.Status,
            ApplicationDate = a.ApplicationDate,
            DecisionDate = a.DecisionDate,
            Notes = a.Notes,
            AccountNumber = a.Account.AccountNumber,
            ProductName = a.Product.Name
        });
    }
}


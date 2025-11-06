using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce code duplication in error handling across controllers
/// </summary>
public static class ControllerErrorHandler
{
    /// <summary>
    /// Handles exceptions and returns a standardized error response
    /// </summary>
    public static ActionResult<ApiResponse<T>> HandleException<T>(
        Exception ex,
        ILogger logger,
        string operation,
        string errorMessage)
    {
        logger.LogError(ex, "Error {Operation}", operation);
        return new ObjectResult(ApiResponse<T>.ErrorResponse(errorMessage))
        {
            StatusCode = 500
        };
    }

    /// <summary>
    /// Creates a standardized 500 error response
    /// </summary>
    public static ActionResult<ApiResponse<T>> CreateErrorResponse<T>(string errorMessage)
    {
        return new ObjectResult(ApiResponse<T>.ErrorResponse(errorMessage))
        {
            StatusCode = 500
        };
    }
}


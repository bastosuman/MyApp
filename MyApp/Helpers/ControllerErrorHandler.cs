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

    /// <summary>
    /// Creates a BadRequest response with error message
    /// </summary>
    public static ActionResult<ApiResponse<T>> BadRequestResponse<T>(string errorMessage)
    {
        return new BadRequestObjectResult(ApiResponse<T>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Creates a NotFound response with error message
    /// </summary>
    public static ActionResult<ApiResponse<T>> NotFoundResponse<T>(string errorMessage)
    {
        return new NotFoundObjectResult(ApiResponse<T>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Creates a 500 response for entity created but not found scenario
    /// </summary>
    public static ActionResult<ApiResponse<T>> EntityCreatedButNotFoundResponse<T>(string entityName)
    {
        return CreateErrorResponse<T>($"{entityName} created but could not be retrieved");
    }
}


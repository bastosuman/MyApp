using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs;

namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in controller action patterns
/// </summary>
public static class ControllerActionHelper
{
    /// <summary>
    /// Wraps a controller action with validation and exception handling
    /// </summary>
    public static async Task<ActionResult<ApiResponse<T>>> ExecuteWithValidationAndErrorHandling<T>(
        ControllerBase controller,
        ILogger logger,
        string operation,
        string errorMessage,
        Func<Task<ActionResult<ApiResponse<T>>>> action)
    {
        try
        {
            var validationError = ControllerHelpers.ValidateModelState<T>(controller);
            if (validationError != null) return validationError;

            return await action();
        }
        catch (Exception ex)
        {
            return ControllerErrorHandler.HandleException<T>(ex, logger, operation, errorMessage);
        }
    }
}


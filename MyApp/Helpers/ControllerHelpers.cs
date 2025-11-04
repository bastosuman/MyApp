using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs;

namespace MyApp.Helpers;

public static class ControllerHelpers
{
    /// <summary>
    /// Validates ModelState and returns BadRequest if invalid
    /// </summary>
    public static ActionResult<ApiResponse<T>>? ValidateModelState<T>(ControllerBase controller) where T : class
    {
        if (!controller.ModelState.IsValid)
        {
            var errors = controller.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return controller.BadRequest(ApiResponse<T>.ErrorResponse("Validation failed", errors));
        }
        return null;
    }
}


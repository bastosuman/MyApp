using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyApp.Core.DTOs;
using MyApp.Helpers;

namespace MyApp.Tests.HelpersTests;

public class ControllerHelpersTests
{
    private class TestController : ControllerBase
    {
    }

    [Fact]
    public void ValidateModelState_ShouldReturnNull_WhenModelStateIsValid()
    {
        // Arrange
        var controller = new TestController();

        // Act
        var result = ControllerHelpers.ValidateModelState<AccountDto>(controller);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateModelState_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var controller = new TestController();
        controller.ModelState.AddModelError("AccountNumber", "Account number is required");

        // Act
        var result = ControllerHelpers.ValidateModelState<AccountDto>(controller);

        // Assert
        Assert.NotNull(result);
        var actionResult = Assert.IsType<ActionResult<ApiResponse<AccountDto>>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Validation failed", response.Message);
        Assert.NotEmpty(response.Errors);
    }

    [Fact]
    public void ValidateModelState_ShouldReturnAllErrors_WhenMultipleErrorsExist()
    {
        // Arrange
        var controller = new TestController();
        controller.ModelState.AddModelError("AccountNumber", "Account number is required");
        controller.ModelState.AddModelError("AccountHolderName", "Account holder name is required");

        // Act
        var result = ControllerHelpers.ValidateModelState<AccountDto>(controller);

        // Assert
        Assert.NotNull(result);
        var actionResult = Assert.IsType<ActionResult<ApiResponse<AccountDto>>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<AccountDto>>(badRequestResult.Value);
        Assert.Equal(2, response.Errors.Count);
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Controllers;
using MyApp.Core.DTOs;
using MyApp.Core.Entities;
using MyApp.Data;
using MyApp.Tests.TestHelpers;

namespace MyApp.Tests.ControllerTests;

public class AuthControllerTests
{
    private static FinancialDbContext CreateDbContext()
    {
        return TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        // Create a test user with hashed password
        var password = "testpassword";
        // Use the same password hashing method as DbInitializer
        var passwordHash = HashPasswordForTest(password);
        var user = new User
        {
            Username = "testuser",
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = password
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.User);
        Assert.Equal("testuser", response.Data.User.Username);
        Assert.Equal("User", response.Data.User.Role);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUsernameIsInvalid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        var loginDto = new LoginDto
        {
            Username = "nonexistent",
            Password = "password"
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Invalid username or password", response.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        var password = "correctpassword";
        var passwordHash = HashPasswordForTest(password);
        var user = new User
        {
            Username = "testuser",
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Invalid username or password", response.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenUsernameIsEmpty()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        var loginDto = new LoginDto
        {
            Username = "",
            Password = "password"
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Username and password are required", response.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsEmpty()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Username and password are required", response.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserIsInactive()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        var password = "testpassword";
        var passwordHash = HashPasswordForTest(password);
        var user = new User
        {
            Username = "inactiveuser",
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = false, // Inactive user
            CreatedDate = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = "inactiveuser",
            Password = password
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(unauthorizedResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_ShouldUpdateLastLoginDate_WhenLoginIsSuccessful()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        var password = "testpassword";
        var passwordHash = HashPasswordForTest(password);
        var user = new User
        {
            Username = "testuser",
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = null
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = password
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
        Assert.True(response.Success);

        // Verify LastLoginDate was updated
        await context.Entry(user).ReloadAsync();
        Assert.NotNull(user.LastLoginDate);
        Assert.True(user.LastLoginDate.Value <= DateTime.UtcNow);
    }

    [Fact]
    public void Logout_ShouldReturnSuccess()
    {
        // Arrange
        using var context = CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<AuthController>();
        var controller = new AuthController(context, logger);

        // Act
        var result = controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    // Helper method to hash password using the same method as DbInitializer
    private static string HashPasswordForTest(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "MyAppSalt2024"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}


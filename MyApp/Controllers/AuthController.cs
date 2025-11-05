using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs;
using MyApp.Data;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FinancialDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(FinancialDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse(
                "Username and password are required.",
                new List<string> { "Invalid credentials" }));
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

        if (user == null || !DbInitializer.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", loginDto.Username);
            return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(
                "Invalid username or password.",
                new List<string> { "Authentication failed" }));
        }

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            Success = true,
            Message = "Login successful",
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            }
        };

        _logger.LogInformation("Successful login for user: {Username}", user.Username);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response));
    }

    [HttpPost("logout")]
    public ActionResult<ApiResponse<object>> Logout()
    {
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Logout successful" }));
    }
}



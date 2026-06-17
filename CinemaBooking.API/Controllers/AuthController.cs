using CinemaBooking.API.Autentification;
using CinemaBooking.API.DTOs.Auth;
using CinemaBooking.API.Services.Auth;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CinemaBooking.API.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService tokenService, IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { Message = "User with this email already exists." });

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }

        // Dodeli rolu (podrazumevano "User", može biti "Admin")
        var allowedRoles = new[] { "Admin", "User" };
        var role = allowedRoles.Contains(request.Role) ? request.Role : "User";
        // C#
var app = builder.Build();

// Ensure identity roles and admin user are seeded before processing requests
app.SeedIdentityAsync().GetAwaiter().GetResult();

app.UseGlobalExceptionHandling();
...await _userManager.AddToRoleAsync(user, role);

        return Ok(new { Message = $"User registered successfully with role '{role}'." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { Message = "Invalid email or password." });

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { Message = "Invalid email or password." });

        var token = await _tokenService.CreateTokenAsync(user);
        return Ok(new LoginResult(token, DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes)));
    }
}
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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

        var allowedRoles = new[] { "Admin", "User" };
        var role = allowedRoles.FirstOrDefault(r =>
            string.Equals(r, request.Role, StringComparison.OrdinalIgnoreCase)) ?? "User";

        if (!await _roleManager.RoleExistsAsync(role))
            return StatusCode(500, new { Message = $"Role '{role}' is not configured in the database." });

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = addToRoleResult.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }

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
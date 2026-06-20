namespace CinemaBooking.Domain.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role = "User"
);

public record LoginRequest(string Email, string Password);
public record LoginResult(string Token, DateTime ExpiresAtUtc);
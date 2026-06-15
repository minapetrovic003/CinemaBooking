using CinemaBooking.Infrastructure.Identity;

namespace CinemaBooking.API.Services.Auth
{
    public interface IJwtTokenService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
    }
}

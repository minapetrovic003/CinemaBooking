using CinemaBooking.Infrastructure.Identity;

namespace CinemaBooking.API.Services
{
    public interface IJwtTokenService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
    }
}

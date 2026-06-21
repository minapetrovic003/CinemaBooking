using CinemaBooking.Domain.DTOs.Users;

namespace CinemaBooking.Application.Repositories;

public interface IUserRepository
{
    Task<UserInfo?> FindByEmailAsync(string email);
    Task<UserInfo?> FindByIdAsync(string userId);
    Task<IReadOnlyDictionary<string, UserInfo>> FindByIdsAsync(IEnumerable<string> userIds);
}
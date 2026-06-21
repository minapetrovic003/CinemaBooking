using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Infrastructure.Identity;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserInfo?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new UserInfo(user.Id, user.GetFullName(), user.Email!);
    }

    public async Task<UserInfo?> FindByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : new UserInfo(user.Id, user.GetFullName(), user.Email!);
    }

    public Task<IReadOnlyDictionary<string, UserInfo>> FindByIdsAsync(IEnumerable<string> userIds)
    {
        var ids = userIds.ToHashSet();
        var result = _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new UserInfo(u.Id, u.GetFullName(), u.Email!))
            .ToDictionary(u => u.Id);

        return Task.FromResult<IReadOnlyDictionary<string, UserInfo>>(result);
    }
}
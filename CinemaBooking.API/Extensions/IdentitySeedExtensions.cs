using CinemaBooking.Infrastructure;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.API.Extensions
{
    public static class IdentitySeedExtensions
    {
        public static async Task SeedIdentityAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<CinemaBookingContext>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (await roleManager.FindByNameAsync(roleName) is null)
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var adminEmail = "admin@cinema.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    FirstName = "Admin",
                    LastName = "Cinema",
                    Email = adminEmail,
                    UserName = adminEmail
                };
                var result = await userManager.CreateAsync(admin, "Admin!123");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Admin user seed failed.");
                }

                if(!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}

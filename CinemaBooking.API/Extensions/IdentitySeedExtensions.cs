// CinemaBooking.API/Extensions/IdentitySeedExtensions.cs
using CinemaBooking.Infrastructure;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.API.Extensions
{
    public static class IdentitySeedExtensions
    {
        public static async Task SeedIdentityAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<CinemaBookingContext>();
            await context.Database.MigrateAsync();

            // ✅ FIX #1: Primijeni migracije automatski pri pokretanju.
            // Bez ovoga Identity tabele ne postoje i admin se ne može kreirati.
            await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Seed role '{roleName}' failed: {errors}");
                    }
                }
            }

            const string adminEmail = "admin@cinema.com";
            const string adminPassword = "Admin!123";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    FirstName = "Admin",
                    LastName = "Cinema",
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true
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

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

            // ✅ FIX #1: Primijeni migracije automatski pri pokretanju.
            // Bez ovoga Identity tabele ne postoje i admin se ne može kreirati.
            await context.Database.MigrateAsync();

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
                    // ✅ FIX: Log specifičnih grešaka umjesto generalne poruke
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Admin user seed failed: {errors}");
                }

                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
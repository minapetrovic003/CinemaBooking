using CinemaBooking.API.CQRS.Behaviors;
using CinemaBooking.API.Middlewares;
using CinemaBooking.API.Services;
using CinemaBooking.API.Services.Auth;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure;
using CinemaBooking.Infrastructure.Identity;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});


builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddDbContext<CinemaBookingContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine)
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IHallService, HallService>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ASP.NET Identity
builder.Services.AddIdentityCore<ApplicationUser>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequiredUniqueChars = 2;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<CinemaBookingContext>();

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = "CinemaBooking.API",
            ValidAudience = "CinemaBooking.Client",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("CinemaBookingTajniKljucDovoljnoDugacak123!"))
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.UseGlobalExceptionHandling();
app.UseRequestLogging();
app.UseIdempotency();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
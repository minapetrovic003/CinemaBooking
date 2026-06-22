using CinemaBooking.Application.Repositories;
using CinemaBooking.Infrastructure;

namespace CinemaBooking.API.BackgroundServices;

/// <summary>
/// Pozadinski servis koji svakih 60 sekundi brise istekle SeatLock zapise iz baze.
/// Registrovan kao IHostedService u Program.cs.
/// Koristi IServiceScopeFactory jer je scoped DbContext nije direktno injectovati
/// u singleton BackgroundService.
/// </summary>
public class SeatLockCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SeatLockCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public SeatLockCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<SeatLockCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SeatLockCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                uow.SeatLocks.CleanupExpiredLocks();
                int saved = uow.SaveChanges();

                if (saved > 0)
                    _logger.LogInformation(
                        "SeatLockCleanup: removed {Count} expired lock(s) at {Time} UTC.",
                        saved, DateTime.UtcNow);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "SeatLockCleanup: error during cleanup.");
            }
        }

        _logger.LogInformation("SeatLockCleanupService stopped.");
    }
}
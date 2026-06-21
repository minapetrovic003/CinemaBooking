using CinemaBooking.Domain.Models;
using CinemaBooking.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

public class SeatLockRepository : ISeatLockRepository
{
    private readonly CinemaBookingContext _context;

    public SeatLockRepository(CinemaBookingContext context)
    {
        _context = context;
    }

    public IEnumerable<SeatLock> GetActiveLocks(long showtimeId) =>
        _context.SeatLocks
            .Where(sl => sl.ShowtimeId == showtimeId && sl.ExpiresAt > DateTime.UtcNow)
            .ToList();

    public IEnumerable<SeatLock> GetActiveLocksForSeats(long showtimeId, IEnumerable<long> seatIds) =>
        _context.SeatLocks
            .Where(sl =>
                sl.ShowtimeId == showtimeId &&
                sl.ExpiresAt > DateTime.UtcNow &&
                seatIds.Contains(sl.SeatId))
            .ToList();

    public void LockSeats(IEnumerable<SeatLock> locks)
    {
        // Pre kreiranja novih, obrisi stare istekle lock-ove
        CleanupExpiredLocks();
        _context.SeatLocks.AddRange(locks);
    }

    public void ReleaseLocksForUser(long showtimeId, string userId)
    {
        var userLocks = _context.SeatLocks
            .Where(sl => sl.ShowtimeId == showtimeId && sl.UserId == userId)
            .ToList();

        _context.SeatLocks.RemoveRange(userLocks);
    }

    public void CleanupExpiredLocks()
    {
        var expired = _context.SeatLocks
            .Where(sl => sl.ExpiresAt <= DateTime.UtcNow)
            .ToList();

        _context.SeatLocks.RemoveRange(expired);
    }

    public bool IsSeatLockedByOther(long seatId, long showtimeId, string currentUserId) =>
        _context.SeatLocks.Any(sl =>
            sl.SeatId == seatId &&
            sl.ShowtimeId == showtimeId &&
            sl.ExpiresAt > DateTime.UtcNow &&
            sl.UserId != currentUserId);
}

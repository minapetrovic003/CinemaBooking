using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Repositories;

public interface ISeatLockRepository
{
    IEnumerable<SeatLock> GetActiveLocks(long showtimeId);
    IEnumerable<SeatLock> GetActiveLocksForSeats(long showtimeId, IEnumerable<long> seatIds);
    void LockSeats(IEnumerable<SeatLock> locks);
    void ReleaseLocksForUser(long showtimeId, string userId);
    void CleanupExpiredLocks();
    bool IsSeatLockedByOther(long seatId, long showtimeId, string currentUserId);
}
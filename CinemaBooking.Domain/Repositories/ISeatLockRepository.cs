namespace CinemaBooking.Domain.Repositories;

public interface ISeatLockRepository
{
    /// <summary>
    /// Vraća aktivne lock-ove (ExpiresAt > now) za dato prikazivanje.
    /// </summary>
    IEnumerable<SeatLock> GetActiveLocks(long showtimeId);

    /// <summary>
    /// Vraća aktivne lock-ove za konkretna sedišta na datom prikazivanju.
    /// </summary>
    IEnumerable<SeatLock> GetActiveLocksForSeats(long showtimeId, IEnumerable<long> seatIds);

    /// <summary>
    /// Kreira lock zapise za listu sedišta. Automatski briše istekle lock-ove
    /// istog korisnika pre kreiranja novih (cleanup).
    /// </summary>
    void LockSeats(IEnumerable<SeatLock> locks);

    /// <summary>
    /// Oslobađa lock-ove korisnika za dato prikazivanje (npr. nakon potvrde booking-a).
    /// </summary>
    void ReleaseLocksForUser(long showtimeId, string userId);

    /// <summary>
    /// Briše sve istekle lock-ove iz baze (poziva se periodično ili pri booking-u).
    /// </summary>
    void CleanupExpiredLocks();

    /// <summary>
    /// Da li je sedište zaključano od strane DRUGOG korisnika (aktivan lock).
    /// </summary>
    bool IsSeatLockedByOther(long seatId, long showtimeId, string currentUserId);
}

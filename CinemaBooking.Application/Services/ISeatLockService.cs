using CinemaBooking.Domain.DTOs.SeatLocks;

namespace CinemaBooking.Application.Services;

public interface ISeatLockService
{
    /// <summary>
    /// Vraca mapu dostupnosti sedista za datu projekciju.
    /// currentUserId = null ako korisnik nije ulogovan.
    /// Returns null ako projekcija ne postoji.
    /// </summary>
    IEnumerable<SeatAvailabilityDto>? GetAvailability(long showtimeId, string? currentUserId);

    /// <summary>
    /// Zaključava odabrana sedišta za korisnika na ograničeno vreme.
    /// userId mora biti interno Identity ID (string GUID).
    /// </summary>
    (SeatLockDto? Result, string? Error, int StatusCode) LockSeats(LockSeatsRequest request, string userId);

    /// <summary>
    /// Oslobađa sve lock-ove datog korisnika za datu projekciju.
    /// Poziva se kada korisnik klikne "Nazad" ili zatvori modal.
    /// </summary>
    (bool Success, string? Error) ReleaseLocks(string userId, long showtimeId);
}
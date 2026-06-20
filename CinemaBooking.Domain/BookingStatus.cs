namespace CinemaBooking.Domain;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Canceled,
    Expired,
    CheckedIn   // Gost je pristigao i QR je skeniran
}
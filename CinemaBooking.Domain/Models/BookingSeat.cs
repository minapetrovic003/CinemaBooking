namespace CinemaBooking.Domain.Models;

public class BookingSeat
{
    public long Id { get; set; }

    public long BookingId { get; set; }
    public long SeatId { get; set; }
    public decimal Price { get; set; }

    public Booking Booking { get; set; } = null!;
    public Seat Seat { get; set; } = null!;

    public bool Validate() => BookingId > 0 && SeatId > 0;

    public string GetSeatLabel() => Seat?.GetSeatLabel() ?? string.Empty;
}
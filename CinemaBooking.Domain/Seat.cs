namespace CinemaBooking.Domain;

public class Seat
{
    public long Id { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public SeatType SeatType { get; set; }

    public long HallId { get; set; }

    public Hall Hall { get; set; } = null!;
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();

    public string GetSeatLabel() => $"{Row}{Number}";
}
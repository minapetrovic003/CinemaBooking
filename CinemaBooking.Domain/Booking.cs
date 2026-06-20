namespace CinemaBooking.Domain;

public class Booking
{
    public long Id { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public long ShowtimeId { get; set; }

    public Showtime Showtime { get; set; } = null!;
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    public Payment? Payment { get; set; }

    public bool Cancel()
    {
        if (Status == BookingStatus.Confirmed || Status == BookingStatus.Pending)
        {
            Status = BookingStatus.Canceled;
            return true;
        }
        return false;
    }

    public bool CheckIn()
    {
        if (Status == BookingStatus.Confirmed)
        {
            Status = BookingStatus.CheckedIn;
            return true;
        }
        return false;
    }

    public bool IsConfirmed() => Status == BookingStatus.Confirmed;

    public decimal CalculateTotalPrice()
    {
        TotalPrice = BookingSeats.Sum(bs => bs.Price);
        return TotalPrice;
    }
}
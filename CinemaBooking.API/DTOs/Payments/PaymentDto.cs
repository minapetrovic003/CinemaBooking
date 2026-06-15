namespace CinemaBooking.API.DTOs.Payments;

public class PaymentDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;

    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime ShowtimeStart { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
}
namespace CinemaBooking.API.DTOs.Payments;

public class CreatePaymentRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowtimeStartTime { get; set; }
    public string Method { get; set; } = string.Empty;
}
namespace CinemaBooking.Domain.Models;

public class Payment
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; }

    public long BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public bool ProcessPayment()
    {
        Status = PaymentStatus.Completed;
        PaymentDate = DateTime.UtcNow;
        return true;
    }

    public bool Refund()
    {
        if (Status == PaymentStatus.Completed)
        {
            Status = PaymentStatus.Refunded;
            PaymentDate = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    public string GetReceipt()
        => $"Payment #{Id} | Amount: {Amount:C} | Status: {Status} | Date: {PaymentDate:g}";
}
namespace CinemaBooking.Domain.DTOs.SeatLocks
{
    public class SeatAvailabilityDto
    {
        public long SeatId { get; set; }
        public string Label { get; set; } = string.Empty;   
        public string Row { get; set; } = string.Empty;   
        public int Number { get; set; }                       
        public string SeatType { get; set; } = string.Empty; 

      
        public string Status { get; set; } = "Available";

        public DateTime? ExpiresAt { get; set; }
        public int? ExpiresInSeconds { get; set; }
    }
}

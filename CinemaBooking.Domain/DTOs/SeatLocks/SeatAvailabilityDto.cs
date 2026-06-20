namespace CinemaBooking.Domain.DTOs.SeatLocks
{
    public class SeatAvailabilityDto
    {
        public long SeatId { get; set; }
        public string Label { get; set; } = string.Empty;   // e.g. "A1", "B5"
        public string Row { get; set; } = string.Empty;      // e.g. "A"
        public int Number { get; set; }                       // e.g. 1
        public string SeatType { get; set; } = string.Empty; // "Standard" | "VIP"

        /// <summary>
        /// "Available" | "Booked" | "Locked" | "MyLock"
        /// MyLock = locked by the currently authenticated user
        /// </summary>
        public string Status { get; set; } = "Available";

        public DateTime? ExpiresAt { get; set; }
        public int? ExpiresInSeconds { get; set; }
    }
}

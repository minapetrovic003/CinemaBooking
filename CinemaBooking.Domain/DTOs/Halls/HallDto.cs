namespace CinemaBooking.Domain.DTOs.Halls;

public class HallDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SeatCount { get; set; }
    public List<SeatInfo> Seats { get; set; } = new();
}

public class SeatInfo
{
    public string Label { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public string SeatType { get; set; } = string.Empty;
}
namespace CinemaBooking.Application.DTOs.Halls;

public class CreateHallRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int? Rows { get; set; }
    public int? SeatsPerRow { get; set; }
}
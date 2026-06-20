namespace CinemaBooking.Domain.Models;

public class Hall
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public bool IsAvailable(DateTime startTime, DateTime endTime)
    {
        return !Showtimes.Any(s =>
            s.StartTime < endTime && s.EndTime > startTime);
    }

    public void GenerateSeats(int rows, int seatsPerRow)
    {
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int r = 0; r < rows && r < letters.Length; r++)
        {
            for (int s = 1; s <= seatsPerRow; s++)
            {
                Seats.Add(new Seat
                {
                    Row = letters[r].ToString(),
                    Number = s,
                    SeatType = SeatType.Standard,
                });
            }
        }
    }
}
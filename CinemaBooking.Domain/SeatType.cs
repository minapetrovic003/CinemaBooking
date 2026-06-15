namespace CinemaBooking.Domain;

public enum SeatType
{
    Standard,
    Vip,
    Wheelchair
}

public static class SeatTypeSurcharge
{
    public static decimal GetSurcharge(SeatType seatType) => seatType switch
    {
        SeatType.Standard => 0m,
        SeatType.Vip => 300m,
        SeatType.Wheelchair => 0m,
        _ => throw new ArgumentOutOfRangeException(nameof(seatType), $"Not expected seat type value: {seatType}")
    };
}
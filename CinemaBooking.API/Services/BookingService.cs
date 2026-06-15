using CinemaBooking.API.DTOs.Bookings;
using CinemaBooking.API.DTOs.Common;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.API.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public PagedResult<BookingDto> GetAll(BookingSearchRequest request)
    {
        var bookings = _uow.Bookings
            .Search(request.UserEmail, request.Status, request.FromDate, request.ToDate)
            .ToList();

        // Skupi sve unique UserIds iz rezultata
        var userIds = bookings.Select(b => b.UserId).Distinct().ToList();

        // Jedan upit ka AspNetUsers za sve korisnike odjednom
        var users = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        var items = bookings
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => MapToDto(b, users.GetValueOrDefault(b.UserId)))
            .ToList();

        return new PagedResult<BookingDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = bookings.Count
        };
    }

    public BookingDto? GetById(long id)
    {
        var booking = _uow.Bookings.GetByIdWithDetails(id);
        if (booking is null) return null;

        var user = _userManager.Users.FirstOrDefault(u => u.Id == booking.UserId);
        return MapToDto(booking, user);
    }

    public (BookingDto? dto, string? errorMessage, int statusCode) Create(CreateBookingRequest request)
    {
        // Tražimo korisnika po emailu kroz UserManager
        var user = _userManager.FindByEmailAsync(request.UserEmail).GetAwaiter().GetResult();
        if (user is null)
            return (null, $"User with email '{request.UserEmail}' not found.", 404);

        var showtime = _uow.Showtimes
            .GetByMovieTitleHallAndStartTime(
                request.MovieTitle, request.HallName, request.ShowtimeStartTime);

        if (showtime is null)
            return (null, "Showtime not found. Check movie title, hall name, and start time.", 404);

        if (showtime.StartTime <= DateTime.UtcNow)
            return (null, "Cannot book past showtimes.", 409);

        var requestedLabels = request.Seats.Select(s => s.ToUpper()).ToList();

        var seats = showtime.Hall.Seats
            .Where(s => requestedLabels.Contains(s.GetSeatLabel().ToUpper()))
            .ToList();

        var notFound = requestedLabels
            .Except(seats.Select(s => s.GetSeatLabel().ToUpper()))
            .ToList();

        if (notFound.Any())
            return (null, $"Seats not found in this hall: {string.Join(", ", notFound)}", 400);

        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtime.Id).ToList();
        var alreadyBooked = seats.Where(s => bookedSeatIds.Contains(s.Id)).ToList();

        if (alreadyBooked.Any())
            return (null, $"Seats already booked: {string.Join(", ", alreadyBooked.Select(s => s.GetSeatLabel()))}", 409);

        var booking = new Booking
        {
            UserId = user.Id,   // string GUID iz Identity
            ShowtimeId = showtime.Id,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            BookingSeats = seats.Select(s => new BookingSeat
            {
                SeatId = s.Id,
                Price = showtime.Price + SeatTypeSurcharge.GetSurcharge(s.SeatType)
            }).ToList()
        };

        booking.CalculateTotalPrice();
        _uow.Bookings.Add(booking);
        _uow.SaveChanges();

        return (new BookingDto
        {
            Id = booking.Id,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            UserFullName = user.GetFullName(),
            UserEmail = user.Email ?? string.Empty,
            MovieTitle = showtime.Movie.Title,
            MovieGenre = showtime.Movie.Genre,
            MovieDurationMinutes = showtime.Movie.DurationMinutes,
            HallName = showtime.Hall.Name,
            ShowtimeStart = showtime.StartTime,
            Seats = seats.Select(s => new BookingSeatDto
            {
                SeatLabel = s.GetSeatLabel(),
                SeatType = s.SeatType.ToString(),
                Price = showtime.Price + SeatTypeSurcharge.GetSurcharge(s.SeatType)
            }).ToList()
        }, null, 201);
    }

    public (bool success, string? errorMessage) Cancel(long id)
    {
        var booking = _uow.Bookings.GetById(id);
        if (booking is null) return (false, null);

        if (!booking.Cancel())
            return (false, "Booking cannot be canceled in its current status.");

        _uow.SaveChanges();
        return (true, null);
    }

    private static BookingDto MapToDto(Booking b, ApplicationUser? user) => new()
    {
        Id = b.Id,
        TotalPrice = b.TotalPrice,
        Status = b.Status.ToString(),
        CreatedAt = b.CreatedAt,
        UserFullName = user?.GetFullName() ?? string.Empty,
        UserEmail = user?.Email ?? string.Empty,
        MovieTitle = b.Showtime?.Movie?.Title ?? string.Empty,
        MovieGenre = b.Showtime?.Movie?.Genre ?? string.Empty,
        MovieDurationMinutes = b.Showtime?.Movie?.DurationMinutes ?? 0,
        HallName = b.Showtime?.Hall?.Name ?? string.Empty,
        ShowtimeStart = b.Showtime?.StartTime ?? DateTime.MinValue,
        Seats = b.BookingSeats?.Select(bs => new BookingSeatDto
        {
            SeatLabel = bs.GetSeatLabel(),
            SeatType = bs.Seat?.SeatType.ToString() ?? string.Empty,
            Price = bs.Price
        }).ToList() ?? new List<BookingSeatDto>()
    };
}
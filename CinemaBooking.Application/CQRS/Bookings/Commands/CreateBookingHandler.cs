using CinemaBooking.Application.CQRS.Bookings.Commands;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Bookings;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class CreateBookingHandler
    : IRequestHandler<CreateBookingCommand, (BookingDto? Dto, string? ErrorMessage, int StatusCode)>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateBookingHandler> _logger;

    public CreateBookingHandler(
        IUnitOfWork uow,
        IUserRepository userRepository,
        ILogger<CreateBookingHandler> logger)
    {
        _uow = uow;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<(BookingDto? Dto, string? ErrorMessage, int StatusCode)> Handle(
        CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return (null, $"User with email '{request.UserEmail}' not found.", 404);

        var showtime = _uow.Showtimes
            .GetByMovieTitleHallAndStartTime(
                request.MovieTitle,
                request.HallName,
                request.ShowtimeStartTime);

        if (showtime is null)
            return (null, "Showtime not found. Check movie title, hall name, and start time.", 404);

        if (showtime.StartTime <= DateTime.UtcNow)
            return (null, "Cannot book past showtimes.", 409);

        var requestedLabels = request.Seats
            .Select(s => s.Trim().ToUpper())
            .Distinct()
            .ToList();

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
            return (null,
                $"Seats already booked: {string.Join(", ", alreadyBooked.Select(s => s.GetSeatLabel()))}",
                409);

        // Booking se kreira kao Pending — potvrđuje se tek nakon plaćanja
        var booking = new Booking
        {
            UserId = user.Id,
            ShowtimeId = showtime.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            BookingSeats = seats.Select(s => new BookingSeat
            {
                SeatId = s.Id,
                Seat = s,
                Price = showtime.Price + SeatTypeSurcharge.GetSurcharge(s.SeatType)
            }).ToList()
        };

        booking.CalculateTotalPrice();
        booking.Showtime = showtime;

        _uow.Bookings.Add(booking);

        try
        {
            _uow.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict while creating booking for showtime {ShowtimeId}.",
                showtime.Id);
            return (null,
                "The seats you selected were just taken by another user. Please refresh and try again.",
                409);
        }

        // Oslobodi lock-ove nakon rezervacije
        try
        {
            _uow.SeatLocks.ReleaseLocksForUser(showtime.Id, user.Id);
            _uow.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not release seat locks for user {UserId} after booking {BookingId}.",
                user.Id, booking.Id);
        }

        var dto = new BookingDto
        {
            Id = booking.Id,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            UserFullName = user.FullName,
            UserEmail = user.Email,
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
        };

        // Email se NE šalje ovdje — šalje se tek nakon plaćanja (PaymentService)
        _logger.LogInformation(
            "Booking #{BookingId} created as Pending for user {Email}. Awaiting payment.",
            booking.Id, user.Email);

        return (dto, null, 201);
    }
}
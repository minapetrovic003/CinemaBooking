using CinemaBooking.Application.CQRS.Bookings.Queries;
using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetBookingByIdHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetByIdWithDetails(request.Id);
        if (booking is null)
            return Task.FromResult<BookingDto?>(null);

        var user = _userManager.Users.FirstOrDefault(u => u.Id == booking.UserId);

        var dto = new BookingDto
        {
            Id = booking.Id,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            UserFullName = user?.GetFullName() ?? string.Empty,
            UserEmail = user?.Email ?? string.Empty,
            MovieTitle = booking.Showtime?.Movie?.Title ?? string.Empty,
            MovieGenre = booking.Showtime?.Movie?.Genre ?? string.Empty,
            MovieDurationMinutes = booking.Showtime?.Movie?.DurationMinutes ?? 0,
            HallName = booking.Showtime?.Hall?.Name ?? string.Empty,
            ShowtimeStart = booking.Showtime?.StartTime ?? DateTime.MinValue,
            Seats = booking.BookingSeats?.Select(bs => new BookingSeatDto
            {
                SeatLabel = bs.GetSeatLabel(),
                SeatType = bs.Seat?.SeatType.ToString() ?? string.Empty,
                Price = bs.Price
            }).ToList() ?? new List<BookingSeatDto>()
        };

        return Task.FromResult<BookingDto?>(dto);
    }
}
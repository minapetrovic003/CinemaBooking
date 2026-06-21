using CinemaBooking.Application.CQRS.Bookings.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Bookings;
using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;

    public GetBookingByIdHandler(IUnitOfWork uow, IUserRepository userRepository)
    {
        _uow = uow;
        _userRepository = userRepository;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetByIdWithDetails(request.Id);
        if (booking is null)
            return null;

        var user = await _userRepository.FindByIdAsync(booking.UserId);

        return new BookingDto
        {
            Id = booking.Id,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            UserFullName = user?.FullName ?? string.Empty,
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
    }
}
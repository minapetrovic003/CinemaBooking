using CinemaBooking.Application.CQRS.Bookings.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Bookings;
using CinemaBooking.Domain.DTOs.Common;
using CinemaBooking.Domain.DTOs.Users;
using CinemaBooking.Domain.Models;
using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class GetAllBookingsHandler : IRequestHandler<GetAllBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;

    public GetAllBookingsHandler(IUnitOfWork uow, IUserRepository userRepository)
    {
        _uow = uow;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<BookingDto>> Handle(
        GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = _uow.Bookings
            .Search(request.UserEmail, request.Status, request.FromDate, request.ToDate)
            .ToList();

        var userIds = bookings.Select(b => b.UserId).Distinct();
        var users = await _userRepository.FindByIdsAsync(userIds);

        var items = bookings
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => MapToDto(b, users.GetValueOrDefault(b.UserId)))
            .ToList();

        var result = new PagedResult<BookingDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = bookings.Count
        };

        return result;
    }

    private static BookingDto MapToDto(Booking b, UserInfo? user) => new()
    {
        Id = b.Id,
        TotalPrice = b.TotalPrice,
        Status = b.Status.ToString(),
        CreatedAt = b.CreatedAt,
        UserFullName = user?.FullName ?? string.Empty,
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
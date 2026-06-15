using CinemaBooking.API.CQRS.Bookings.Queries;
using CinemaBooking.API.DTOs.Bookings;
using CinemaBooking.API.DTOs.Common;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.API.CQRS.Bookings.Handlers;

public class GetAllBookingsHandler : IRequestHandler<GetAllBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllBookingsHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public Task<PagedResult<BookingDto>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = _uow.Bookings
            .Search(request.UserEmail, request.Status, request.FromDate, request.ToDate)
            .ToList();

        var userIds = bookings.Select(b => b.UserId).Distinct().ToList();

        var users = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

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

        return Task.FromResult(result);
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
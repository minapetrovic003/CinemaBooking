using CinemaBooking.API.DTOs.Bookings;
using CinemaBooking.API.DTOs.Common;
using MediatR;

namespace CinemaBooking.API.CQRS.Bookings.Queries;

public record GetAllBookingsQuery(
    string? UserEmail,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page,
    int PageSize
) : IRequest<PagedResult<BookingDto>>;
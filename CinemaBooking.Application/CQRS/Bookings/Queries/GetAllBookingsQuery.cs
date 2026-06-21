using CinemaBooking.Domain.DTOs.Bookings;
using CinemaBooking.Domain.DTOs.Common;
using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Queries;

public record GetAllBookingsQuery(
    string? UserEmail,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page,
    int PageSize
) : IRequest<PagedResult<BookingDto>>;
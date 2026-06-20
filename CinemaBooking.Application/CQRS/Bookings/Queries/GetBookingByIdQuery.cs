using CinemaBooking.Application.DTOs.Bookings;
using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Queries;

public record GetBookingByIdQuery(long Id) : IRequest<BookingDto?>;
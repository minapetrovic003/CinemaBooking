using CinemaBooking.API.DTOs.Bookings;
using MediatR;

namespace CinemaBooking.API.CQRS.Bookings.Queries;

public record GetBookingByIdQuery(long Id) : IRequest<BookingDto?>;
using CinemaBooking.Application.DTOs.Bookings;
using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Commands;

// Vraća (BookingDto?, errorMessage?, statusCode)
public record CreateBookingCommand(
    string UserEmail,
    string MovieTitle,
    string HallName,
    DateTime ShowtimeStartTime,
    List<string> Seats
) : IRequest<(BookingDto? Dto, string? ErrorMessage, int StatusCode)>;
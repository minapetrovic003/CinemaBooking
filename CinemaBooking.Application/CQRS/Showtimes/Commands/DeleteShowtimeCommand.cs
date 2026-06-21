using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Commands;

public record DeleteShowtimeCommand(long Id) : IRequest<bool>;
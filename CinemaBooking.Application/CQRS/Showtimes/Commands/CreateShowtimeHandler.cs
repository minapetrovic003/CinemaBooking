using CinemaBooking.Application.CQRS.Showtimes.Commands;
using CinemaBooking.Application.CQRS.Showtimes.Handlers;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Showtimes;
using CinemaBooking.Domain.Models;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Handlers;

public class CreateShowtimeHandler : IRequestHandler<CreateShowtimeCommand, ShowtimeDto>
{
    private readonly IUnitOfWork _uow;

    public CreateShowtimeHandler(IUnitOfWork uow) => _uow = uow;

    public Task<ShowtimeDto> Handle(CreateShowtimeCommand request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetByTitle(request.MovieTitle)
            ?? throw new KeyNotFoundException($"Movie '{request.MovieTitle}' not found.");

        var hall = _uow.Halls.GetByNameWithShowtimes(request.HallName)
            ?? throw new KeyNotFoundException($"Hall '{request.HallName}' not found.");

        var endTime = request.StartTime.AddMinutes(movie.DurationMinutes);

        if (!hall.IsAvailable(request.StartTime, endTime))
            throw new InvalidOperationException("Hall is not available at this time.");

        var showtime = new Showtime
        {
            StartTime = request.StartTime,
            EndTime = endTime,
            Price = request.Price,
            MovieId = movie.Id,
            HallId = hall.Id
        };

        _uow.Showtimes.Add(showtime);
        _uow.SaveChanges();

        var dto = new ShowtimeDto
        {
            Id = showtime.Id,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            MovieTitle = movie.Title,
            MovieGenre = movie.Genre,
            MovieDurationMinutes = movie.DurationMinutes,
            HallName = hall.Name,
            HallCapacity = hall.Capacity,
            BookingCount = 0,
            AvailableSeats = hall.Capacity
        };

        return Task.FromResult(dto);
    }
}
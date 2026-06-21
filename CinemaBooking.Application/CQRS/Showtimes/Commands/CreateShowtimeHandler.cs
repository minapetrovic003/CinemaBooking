using CinemaBooking.Application.CQRS.Showtimes.Commands;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Showtimes;
using CinemaBooking.Domain.Models;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Handlers;

public class CreateShowtimeHandler
    : IRequestHandler<CreateShowtimeCommand, (ShowtimeDto? Dto, string? ErrorMessage, int StatusCode)>
{
    private readonly IUnitOfWork _uow;

    public CreateShowtimeHandler(IUnitOfWork uow) => _uow = uow;

    public Task<(ShowtimeDto? Dto, string? ErrorMessage, int StatusCode)> Handle(
        CreateShowtimeCommand request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetByTitle(request.MovieTitle);
        if (movie is null)
            return Task.FromResult<(ShowtimeDto?, string?, int)>(
                (null, $"Movie '{request.MovieTitle}' not found.", 404));

        var hall = _uow.Halls.GetByNameWithShowtimes(request.HallName);
        if (hall is null)
            return Task.FromResult<(ShowtimeDto?, string?, int)>(
                (null, $"Hall '{request.HallName}' not found.", 404));

        var endTime = request.StartTime.AddMinutes(movie.DurationMinutes);

        if (!hall.IsAvailable(request.StartTime, endTime))
            return Task.FromResult<(ShowtimeDto?, string?, int)>(
                (null, "Hall is not available at this time.", 409));

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

        return Task.FromResult<(ShowtimeDto?, string?, int)>((dto, null, 201));
    }
}
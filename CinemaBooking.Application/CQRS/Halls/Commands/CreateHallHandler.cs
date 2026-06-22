using CinemaBooking.Application.CQRS.Halls.Commands;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Halls;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Halls.Handlers;

public class CreateHallHandler
    : IRequestHandler<CreateHallCommand, (HallDto? Dto, string? ErrorMessage, int StatusCode)>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateHallHandler> _logger;

    public CreateHallHandler(IUnitOfWork uow, ILogger<CreateHallHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<(HallDto? Dto, string? ErrorMessage, int StatusCode)> Handle(
        CreateHallCommand request, CancellationToken cancellationToken)
    {
        if (_uow.Halls.GetByName(request.Name) is not null)
            return (null, $"Hall with name '{request.Name}' already exists.", 409);

        var hall = new Hall
        {
            Name = request.Name,
            Capacity = request.Capacity
        };

        if (request.Rows.HasValue && request.SeatsPerRow.HasValue)
            hall.GenerateSeats(request.Rows.Value, request.SeatsPerRow.Value);

        _uow.Halls.Add(hall);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Hall #{HallId} '{HallName}' created successfully.", hall.Id, hall.Name);

        var dto = new HallDto
        {
            Id = hall.Id,
            Name = hall.Name,
            Capacity = hall.Capacity,
            SeatCount = hall.Seats.Count
        };

        return (dto, null, 201);
    }
}
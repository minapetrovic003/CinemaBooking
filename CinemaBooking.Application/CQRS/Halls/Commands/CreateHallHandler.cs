using CinemaBooking.Application.CQRS.Bookings.Commands;
using CinemaBooking.Application.CQRS.Bookings.Handlers;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Bookings;
using CinemaBooking.Domain.DTOs.Halls;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Halls.Commands
{
    public class CreateHallHandler
        : IRequestHandler<CreateHallCommand, (HallDto? Dto, string? ErrorMessage, int StatusCode)>
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CreateHallHandler> _logger;

        public CreateHallHandler(
            IUnitOfWork uow,
            IUserRepository userRepository,
            ILogger<CreateHallHandler> logger)
        {
            _uow = uow;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(HallDto? Dto, string? ErrorMessage, int StatusCode)> Handle(
            CreateHallCommand request, CancellationToken cancellationToken)
        {
            if (_uow.Halls.GetByName(request.Name) is not null)
                return (null, $"Hall with name '{request.Name}' already exists.", 404);

            var hall = new Hall
            {
                Name = request.Name,
                Capacity = request.Capacity
            };

            if (request.Rows.HasValue && request.SeatsPerRow.HasValue)
                hall.GenerateSeats(request.Rows.Value,
                    request.SeatsPerRow.Value);

            _uow.Halls.Add(hall);

            try
            {
                _uow.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict while creating hall!");
                return (null,
                    "Hall can not be created!",
                    409);
            }

            var HallDtoCreated = new HallDto
            {
                Id = hall.Id,
                Name = hall.Name,
                Capacity = hall.Capacity,
                SeatCount = hall.Seats.Count
            };
            _logger.LogInformation(
            "Hall is created!"
           );

            return (HallDtoCreated, null, 201);

        }

    }
}
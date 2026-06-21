using CinemaBooking.Domain.DTOs.Halls;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR;


namespace CinemaBooking.Application.CQRS.Halls.Commands
{
    public record CreateHallCommand
    (
        string Name,
        int Capacity,
        int? Rows,
        int? SeatsPerRow
    ) : IRequest<(HallDto? Dto, string? ErrorMessage, int StatusCode)>;
}

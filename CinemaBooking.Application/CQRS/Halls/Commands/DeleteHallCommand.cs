using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Halls.Commands
{
    public record DeleteHallCommand(long Id) : IRequest<bool>;
}

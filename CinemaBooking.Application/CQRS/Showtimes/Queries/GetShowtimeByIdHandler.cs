using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Showtimes;
using CinemaBooking.Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Showtimes.Queries
{
    public class GetShowtimeByIdHandler : IRequestHandler<GetShowtimeByIdQuery, ShowtimeDto?>
    {
        private readonly IUnitOfWork _uow;

        public GetShowtimeByIdHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Task<ShowtimeDto?> Handle(GetShowtimeByIdQuery request, CancellationToken cancellationToken)
        {
            var s = _uow.Showtimes.GetByIdWithDetails(request.Id);
            if (s is null) return Task.FromResult<ShowtimeDto?>(null);

            return Task.FromResult<ShowtimeDto?>(GetAllShowtimesHandler.MapToDto(s));
        }

    }
}

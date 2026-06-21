using CinemaBooking.Application.CQRS.Movies.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Halls;
using CinemaBooking.Domain.DTOs.Movies;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Halls.Queries
{
    public class GetHallByIdHendler  : IRequestHandler<GetHallByIdQuery, HallDto?>
    {
        private readonly IUnitOfWork _uow;

        public GetHallByIdHendler(IUnitOfWork uow) => _uow = uow;

        public Task<HallDto?> Handle(GetHallByIdQuery request, CancellationToken cancellationToken)
        {
            var h = _uow.Halls.GetById(request.id);

            if (h is null)
                return Task.FromResult<HallDto?>(null);

            var dto = new HallDto
            {
                Id = h.Id,
                Name = h.Name,
                Capacity = h.Capacity,
                SeatCount = h.Seats?.Count ?? 0
            };

            return Task.FromResult<HallDto?>(dto);
        }


    }
}

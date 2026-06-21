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
    public class GetHallByIdHandler  : IRequestHandler<GetHallByIdQuery, HallDto?>
    {
        private readonly IUnitOfWork _uow;

        public GetHallByIdHandler(IUnitOfWork uow) => _uow = uow;

        public Task<HallDto?> Handle(GetHallByIdQuery request, CancellationToken cancellationToken)
        {
            var hall = _uow.Halls.GetByIdWithSeats(request.id);

            if (hall is null)
                return Task.FromResult<HallDto?>(null);

            var dto = new HallDto
            {
                Id = hall.Id,
                Name = hall.Name,
                Capacity = hall.Capacity,
                SeatCount = hall.Seats.Count,
                Seats = hall.Seats.Select(s => new SeatInfo
                {
                    Label = s.GetSeatLabel(),
                    Row = s.Row,
                    Number = s.Number,
                    SeatType = s.SeatType.ToString()
                }).ToList()
            };

            return Task.FromResult<HallDto?>(dto);
        }


    }
}

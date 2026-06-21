using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Halls;
using MediatR;

namespace CinemaBooking.Application.CQRS.Halls.Queries
{
    public class GetAllHallsHendler
        : IRequestHandler<GetAllHallsQuery, List<HallDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetAllHallsHendler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Task<List<HallDto>> Handle(
            GetAllHallsQuery request,
            CancellationToken cancellationToken)
        {
            var result = _uow.Halls.GetAll()
                .Select(h => new HallDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Capacity = h.Capacity,
                    SeatCount = h.Seats?.Count ?? 0
                })
                .ToList();

            return Task.FromResult(result);
        }
    }
}
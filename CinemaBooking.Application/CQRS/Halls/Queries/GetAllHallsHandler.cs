using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Halls;
using MediatR;

namespace CinemaBooking.Application.CQRS.Halls.Queries
{
    public class GetAllHallsHandler
        : IRequestHandler<GetAllHallsQuery, IEnumerable<HallDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetAllHallsHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Task<IEnumerable<HallDto>> Handle(
            GetAllHallsQuery request,
            CancellationToken cancellationToken)
        {
            var halls = _uow.Halls.GetAll()
                .Select(h => new HallDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Capacity = h.Capacity,
                    SeatCount = h.Seats?.Count ?? 0
                });

            return Task.FromResult(halls);
        }
    }
}
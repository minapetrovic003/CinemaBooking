using CinemaBooking.Application.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Halls.Commands
{
    public class DeleteHallHandler : IRequestHandler<DeleteHallCommand, bool>
    {
        private readonly IUnitOfWork _uow;

        public DeleteHallHandler(IUnitOfWork uow) => _uow = uow;

        public Task<bool> Handle(DeleteHallCommand request, CancellationToken cancellationToken)
        {
            var hall = _uow.Halls.GetById(request.Id);
            if (hall is null)
                return Task.FromResult(false);

            _uow.Halls.Remove(hall);
            _uow.SaveChanges();
            return Task.FromResult(true);
        }
    }
}

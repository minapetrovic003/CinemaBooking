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

        public async Task<bool> Handle(DeleteHallCommand request, CancellationToken cancellationToken)
        {
            var hall = _uow.Halls.GetById(request.Id);
            if (hall is null)
                return false;

            _uow.Halls.Remove(hall);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}

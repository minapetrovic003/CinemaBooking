using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Payment? GetByIdWithDetails(long id);
    IEnumerable<Payment> GetAllWithDetails();
}
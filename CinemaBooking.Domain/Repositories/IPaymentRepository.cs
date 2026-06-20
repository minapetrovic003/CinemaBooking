using CinemaBooking.Domain.Models;

namespace CinemaBooking.Domain.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Payment? GetByIdWithDetails(long id);
    IEnumerable<Payment> GetAllWithDetails();
}
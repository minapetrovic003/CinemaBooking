using CinemaBooking.Application.CQRS.Payments.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Payments;
using MediatR;

namespace CinemaBooking.Application.CQRS.Payments.Handlers;

public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;

    public GetPaymentByIdHandler(IUnitOfWork uow, IUserRepository userRepository)
    {
        _uow = uow;
        _userRepository = userRepository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = _uow.Payments.GetByIdWithDetails(request.Id);
        if (payment is null) return null;

        var user = payment.Booking is not null
            ? await _userRepository.FindByIdAsync(payment.Booking.UserId)
            : null;

        return GetAllPaymentsHandler.MapToDto(payment, user?.FullName, user?.Email);
    }
}
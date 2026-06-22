using CinemaBooking.Application.CQRS.Payments.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Payments;
using CinemaBooking.Domain.Models;
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

        return MapToDto(payment, user?.FullName, user?.Email);
    }

    private static PaymentDto MapToDto(Payment p, string? fullName, string? email) => new()
    {
        Id = p.Id,
        Amount = p.Amount,
        PaymentDate = p.PaymentDate,
        Status = p.Status.ToString(),
        Method = p.Method.ToString(),
        UserFullName = fullName ?? string.Empty,
        UserEmail = email ?? string.Empty,
        MovieTitle = p.Booking?.Showtime?.Movie?.Title ?? string.Empty,
        ShowtimeStart = p.Booking?.Showtime?.StartTime ?? DateTime.MinValue,
        BookingStatus = p.Booking?.Status.ToString() ?? string.Empty
    };
}
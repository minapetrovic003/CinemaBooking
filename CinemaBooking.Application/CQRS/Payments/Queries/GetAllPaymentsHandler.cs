using CinemaBooking.Application.CQRS.Payments.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Payments;
using CinemaBooking.Domain.Models;
using MediatR;

namespace CinemaBooking.Application.CQRS.Payments.Handlers;

public class GetAllPaymentsHandler : IRequestHandler<GetAllPaymentsQuery, IEnumerable<PaymentDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;

    public GetAllPaymentsHandler(IUnitOfWork uow, IUserRepository userRepository)
    {
        _uow = uow;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<PaymentDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = _uow.Payments.GetAllWithDetails().ToList();
        var result = new List<PaymentDto>();

        foreach (var p in payments)
        {
            var user = p.Booking is not null
                ? await _userRepository.FindByIdAsync(p.Booking.UserId)
                : null;
            result.Add(MapToDto(p, user?.FullName, user?.Email));
        }

        return result;
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
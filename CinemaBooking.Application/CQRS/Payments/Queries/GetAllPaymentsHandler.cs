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

    public async Task<IEnumerable<PaymentDto>> Handle(
        GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = _uow.Payments.GetAllWithDetails().ToList();

        // Jedan DB poziv za sve korisnike umesto N poziva u foreach petlji
        var userIds = payments
            .Where(p => p.Booking is not null)
            .Select(p => p.Booking!.UserId)
            .Distinct();

        var users = await _userRepository.FindByIdsAsync(userIds);

        return payments.Select(p =>
        {
            var userId = p.Booking?.UserId;
            var user = userId is not null && users.TryGetValue(userId, out var u) ? u : null;
            return MapToDto(p, user?.FullName, user?.Email);
        }).ToList();
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
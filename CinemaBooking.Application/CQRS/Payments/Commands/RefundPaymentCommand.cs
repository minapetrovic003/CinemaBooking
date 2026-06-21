using MediatR;

namespace CinemaBooking.Application.CQRS.Payments.Commands;

public record RefundPaymentCommand(long Id) : IRequest<bool>;
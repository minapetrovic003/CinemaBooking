using CinemaBooking.Domain.DTOs.Payments;
using MediatR;

namespace CinemaBooking.Application.CQRS.Payments.Queries;

public record GetAllPaymentsQuery() : IRequest<IEnumerable<PaymentDto>>;
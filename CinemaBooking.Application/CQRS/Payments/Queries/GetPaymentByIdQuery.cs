using CinemaBooking.Domain.DTOs.Payments;
using MediatR;

namespace CinemaBooking.Application.CQRS.Payments.Queries;

public record GetPaymentByIdQuery(long Id) : IRequest<PaymentDto?>;
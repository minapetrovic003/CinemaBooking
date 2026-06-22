using CinemaBooking.Application.CQRS.Halls.Commands;
using FluentValidation;

public class UpdateSeatTypeCommandValidator : AbstractValidator<UpdateSeatTypeCommand>
{
    private static readonly string[] ValidTypes = { "Standard", "Vip", "Wheelchair" };

    public UpdateSeatTypeCommandValidator()
    {
        RuleFor(x => x.SeatType)
            .NotEmpty().WithMessage("SeatType is required.")
            .Must(t => ValidTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SeatType must be Standard, Vip, or Wheelchair.");
    }
}
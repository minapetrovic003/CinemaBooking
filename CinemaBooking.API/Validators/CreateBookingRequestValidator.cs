using CinemaBooking.API.DTOs.Bookings;
using FluentValidation;

namespace CinemaBooking.API.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(b => b.UserEmail)
            .NotEmpty().WithMessage("User email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(b => b.MovieTitle)
            .NotEmpty().WithMessage("Movie title is required.");

        RuleFor(b => b.HallName)
            .NotEmpty().WithMessage("Hall name is required.");

        RuleFor(b => b.ShowtimeStartTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Showtime must be in the future.");

        RuleFor(b => b.Seats)
            .NotEmpty().WithMessage("At least one seat must be selected.")
            .Must(s => s.Count <= 10).WithMessage("Cannot book more than 10 seats at once.")
            .Must(s => s.Distinct(StringComparer.OrdinalIgnoreCase).Count() == s.Count)
                .WithMessage("Duplicate seats are not allowed.");

        RuleForEach(b => b.Seats)
            .NotEmpty().WithMessage("Seat label cannot be empty.")
            .Matches(@"^[A-Z]\d+$").WithMessage("Seat label must be in format A1, B5, etc.");
    }
}
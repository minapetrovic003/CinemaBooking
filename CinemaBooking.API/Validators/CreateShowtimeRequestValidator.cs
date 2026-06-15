using CinemaBooking.API.DTOs.Showtimes;
using FluentValidation;

namespace CinemaBooking.API.Validators;

public class CreateShowtimeRequestValidator : AbstractValidator<CreateShowtimeRequest>
{
    public CreateShowtimeRequestValidator()
    {
        RuleFor(s => s.MovieTitle)
            .NotEmpty().WithMessage("Movie title is required.");

        RuleFor(s => s.HallName)
            .NotEmpty().WithMessage("Hall name is required.");

        RuleFor(s => s.StartTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Showtime must be in the future.");

        RuleFor(s => s.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}
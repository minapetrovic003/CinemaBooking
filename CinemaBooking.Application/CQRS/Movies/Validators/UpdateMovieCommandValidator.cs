using CinemaBooking.Application.CQRS.Movies.Commands;
using FluentValidation;

namespace CinemaBooking.Application.CQRS.Movies.Validators;

public class UpdateMovieCommandValidator : AbstractValidator<UpdateMovieCommand>
{
    public UpdateMovieCommandValidator()
    {
        RuleFor(m => m.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(m => m.Genre)
            .NotEmpty().WithMessage("Genre is required.")
            .MaximumLength(100);

        RuleFor(m => m.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.")
            .LessThanOrEqualTo(300).WithMessage("Duration cannot exceed 300 minutes.");

        RuleFor(m => m.Rating)
            .InclusiveBetween(0, 10).WithMessage("Rating must be between 0 and 10.");
    }
}
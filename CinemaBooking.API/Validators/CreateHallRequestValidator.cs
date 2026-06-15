using CinemaBooking.API.DTOs.Halls;
using FluentValidation;

namespace CinemaBooking.API.Validators
{
    public class CreateHallRequestValidator : AbstractValidator<CreateHallRequest>
    {
        public CreateHallRequestValidator()
        {
            RuleFor(h => h.Name)
            .NotEmpty().WithMessage("Hall name is required.")
            .MaximumLength(100).WithMessage("Hall name cannot exceed 100 characters.");

            RuleFor(h => h.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            When(h => h.Rows.HasValue || h.SeatsPerRow.HasValue, () =>
            {
                RuleFor(h => h.Rows)
                    .NotNull().WithMessage("Rows is required when SeatsPerRow is provided.")
                    .GreaterThan(0).WithMessage("Rows must be greater than 0.")
                    .LessThanOrEqualTo(26).WithMessage("Maximum 26 rows (A-Z).");

                RuleFor(h => h.SeatsPerRow)
                    .NotNull().WithMessage("SeatsPerRow is required when Rows is provided.")
                    .GreaterThan(0).WithMessage("SeatsPerRow must be greater than 0.");

                RuleFor(h => h)
                    .Must(h => h.Rows.HasValue && h.SeatsPerRow.HasValue &&
                               h.Rows.Value * h.SeatsPerRow.Value == h.Capacity)
                    .WithMessage(h =>
                        $"Rows ({h.Rows}) * SeatsPerRow ({h.SeatsPerRow}) must equal Capacity ({h.Capacity}).")
                    .When(h => h.Rows.HasValue && h.SeatsPerRow.HasValue);
            });
        }
    }
}

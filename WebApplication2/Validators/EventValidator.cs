using FluentValidation;
using WebApplication2.Dtos.EventDtos;

namespace WebApplication2.Validators
{
    public class EventValidator : AbstractValidator<EventCreateDto>
    {
        public EventValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Date)
                .Must(d => d > DateTime.Now)
                .WithMessage("Date must be in the future");

            RuleFor(x => x.Location)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}

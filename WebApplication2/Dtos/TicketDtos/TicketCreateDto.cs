
using FluentValidation;

namespace WebApplication2.Dtos.TicketDtos
{
    public class TicketCreateDto
    {
        public string Type { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public int EventId { get; set; }
    }
    public class TicketCreateDtoValidator : AbstractValidator<TicketCreateDto>
    {
        public TicketCreateDtoValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Ticket type is required.")
                .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.QuantityAvailable)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity cannot be negative.");

            RuleFor(x => x.EventId)
                .GreaterThan(0)
                .WithMessage("Invalid event id.");
        }
    }
}

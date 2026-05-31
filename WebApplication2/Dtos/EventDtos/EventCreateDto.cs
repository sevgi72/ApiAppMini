using FluentValidation;

namespace WebApplication2.Dtos.EventDtos
{
    public class EventCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }= null!;
        public DateTime Date { get; set; }
        public string Location { get; set; }= null!;
        public int OrganizerId { get; set; }
        //public IFormFile Photo { get; set; }= null!;
    }
    public class EventCreateDtoValidator : AbstractValidator<EventCreateDto>
    {
        public EventCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150)
                .WithMessage("Title cannot exceed 150 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MinimumLength(10)
                .WithMessage("Description must be at least 10 characters.");

            RuleFor(x => x.Date)
                .GreaterThan(DateTime.Now)
                .WithMessage("Event date must be in the future.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required.")
                .MaximumLength(200)
                .WithMessage("Location cannot exceed 200 characters.");

            RuleFor(x => x.OrganizerId)
                .GreaterThan(0)
                .WithMessage("Invalid organizer id.");

            //RuleFor(x => x.Photo)
            //    .NotNull().WithMessage("Photo is required.");

            //RuleFor(x => x.Photo.Length)
            //    .LessThanOrEqualTo(5 * 1024 * 1024)
            //    .WithMessage("Photo size cannot exceed 5 MB.");

            //RuleFor(x => x.Photo.ContentType)
            //    .Must(type =>
            //        type == "image/png" ||
            //        type == "image/jpeg" ||
            //        type == "image/jpg")
            //    .WithMessage("Only png, jpg and jpeg files are allowed.");
        }
    }
}

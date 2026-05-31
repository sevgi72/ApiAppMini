using FluentValidation;

namespace WebApplication2.Dtos.OrganizerDtos
{
    public class OrganizerCreateDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        //public IFormFile Logo { get; set; }
    }
    public class Validator : AbstractValidator<OrganizerCreateDto>
    {

        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(@"^\+994\d{9}$")
                .WithMessage("Phone must be in +994XXXXXXXXX format.");

            //RuleFor(x => x.Logo)
            //    .NotNull().WithMessage("Logo is required.");

            //RuleFor(x => x.Logo.Length)
            //    .LessThanOrEqualTo(2 * 1024 * 1024)
            //    .WithMessage("Logo size cannot exceed 2 MB.");

            //RuleFor(x => x.Logo.ContentType)
            //    .Must(type =>
            //        type == "image/png" ||
            //        type == "image/jpeg" ||
            //        type == "image/jpg")
            //    .WithMessage("Only png, jpg, jpeg files are allowed.");
        }
    }
}

using FluentValidation;

namespace WebApplication2.Dtos.User
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;

    }
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty()
                .WithMessage("Full name is required.");
            RuleFor(x => x.UserName).NotEmpty()
                .WithMessage("Username is required.");
            RuleFor(x => x.Email).NotEmpty()
                .EmailAddress().WithMessage("Valid email is required.");
            RuleFor(x => x.Password).NotEmpty()
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");
        }
    }
}

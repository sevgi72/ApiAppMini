using FluentValidation;

namespace WebApplication2.Dtos
{
    public class ResetPasswordDto
    {
        public  string Email { get; set; }
        public  string Token { get; set; }
        public  string NewPassword { get; set; }
        public  string ConfirmNewPassword { get; set; }
    }
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Token)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6)
                .Matches("[A-Z]").WithMessage("At least one uppercase letter required.")
                .Matches("[a-z]").WithMessage("At least one lowercase letter required.")
                .Matches("[0-9]").WithMessage("At least one digit required.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");
        }
    }
}

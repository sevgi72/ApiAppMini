using FluentValidation;
using WebApplication2.Dtos;

namespace WebApplication2.Validators
{
    
        public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
        {
            public ForgotPasswordValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("Invalid email format.");
            }

        }
    
}

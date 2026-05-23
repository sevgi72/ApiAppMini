using FluentValidation;

namespace WebApplication2.Dtos.User
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    //todo: abstractvalidatorun icindeki ozelliklere baxS
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("pasword is required");
        }
    }
}

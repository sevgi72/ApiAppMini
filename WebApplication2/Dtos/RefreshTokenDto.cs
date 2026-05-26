using FluentValidation;

namespace WebApplication2.Dtos
{
    public class RefreshTokenDto
    {
        public string Username { get; set; }
        public string RefreshToken { get; set; }
    }
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty();

            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}

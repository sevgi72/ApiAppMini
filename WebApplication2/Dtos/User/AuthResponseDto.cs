// Dtos/User/AuthResponseDto.cs
namespace WebApplication2.Dtos.User
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
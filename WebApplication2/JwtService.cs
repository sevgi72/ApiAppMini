using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication2.Models;

namespace WebApplication2
{
    public class JwtService
    {
        public string GenerateToken(AppUser user, IList<string> roles, IConfiguration config)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Fullname",user.FullName)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            //bu acarin bitelarini goturub tokeni generate edecek servicee gonderirik ve tokeni aliriq
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            //credentiallarin icinde acari ve algoritmi gonderirik ve bu credentiallar tokeni generate edecek servicee gonderilir
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                claims: claims,
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creds
                );
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return token;

        }
    }
}

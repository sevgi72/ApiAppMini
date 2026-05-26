using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using WebApplication2.Dtos;
using WebApplication2.Dtos.User;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        IValidator<RegisterDto> validator,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
        IConfiguration config,
        JwtService jwtService
        ) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var validationResult = validator.Validate(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var user = await userManager.FindByNameAsync(registerDto.UserName);
            if (user is not null)
            {
                return BadRequest("UserName already exists");
            }
            user = mapper.Map<AppUser>(registerDto);
            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await userManager.AddToRoleAsync(user, "Member");
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme
            );


            return Ok(new
            {
                message = "user registered successfully",
                confirmLink
            });


        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.Username)
                       ?? await userManager.FindByEmailAsync(loginDto.Username);

            if (user is null)
                return BadRequest("invalid username or password");

            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
                return BadRequest("invalid username or password");

            if (!user.EmailConfirmed)
            {
                return BadRequest("Please confirm your email.");
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);
            var refreshToken = Guid.NewGuid().ToString();
            await userManager.SetAuthenticationTokenAsync(user, "ApiApp", "RefreshToken", refreshToken);

            return Ok(new
            {
                //accessToken = jwtService.GenerateToken(user, roles),
                Token = token,
                RefreshToken = refreshToken // front bunu yadda saxlayir ve access tokeni bitdikde refresh tokeni ile yeni token isteyir
            });
        }
        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            //useauthenticate middlewaresi ile userin icindeki melumatlar doldurulur
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.Identity?.Name;
            var fullname = User.FindFirstValue("Fullname");
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            return Ok(new
            {
                userId,
                username,
                fullname,
                roles
            });
        }

        //---------------------------------------------------------------------------------------



    
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Ok("If the email exists in the system, a password reset token has been sent.");
            }
            // identity unique token yardir ve bu tokenin vaxti var, real isde emaile gedir, test ucun response da qaytaririq
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            return Ok(new { Message = "Token created successfully.", Token = token });
        }




        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // identity tokeni yoxlayir duzdurse ve vaxti kecmemisdirse yeni sifre ile resetleyir
            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (result.Succeeded)
            {
                return Ok("Your password has been successfully reset.");
            }

            
            return BadRequest(result.Errors);
        }

        //------------------------------------------------------------------------------------


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound("User not found");
            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (FormatException)
            {
                return BadRequest("Invalid token format");
            }
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return BadRequest("Invalid token");

            return Ok("Email confirmed successfully");
        }

        //------------------------------------------------------------------------------------------------------------------


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.Username)
                       ?? await userManager.FindByEmailAsync(dto.Username);
            if (user == null) return BadRequest("User not found.");

            // saved dbdaki tokeni oxuyuruq
            var savedRefreshToken = await userManager.GetAuthenticationTokenAsync(user, "ApiApp", "RefreshToken");

           
            if (savedRefreshToken != dto.RefreshToken)
            {
                return BadRequest("Unacceptable refresh token.");
            }

           
            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = jwtService.GenerateToken(user, roles);
            var newRefreshToken = Guid.NewGuid().ToString();

            // refresh token mentiqini burda yaziriq
            await userManager.SetAuthenticationTokenAsync(user, "ApiApp", "RefreshToken", newRefreshToken);

            return Ok(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }


        //[HttpGet]
        //public async Task<IActionResult> CreateRole()
        //{
        //    await roleManager.CreateAsync(new IdentityRole("Member"));
        //    await roleManager.CreateAsync(new IdentityRole("Admin"));

        //    return Ok();
        //}
    }
}

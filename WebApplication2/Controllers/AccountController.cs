using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using WebApplication2.Common;
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
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Fail(errors));
            }

            var user = await userManager.FindByNameAsync(registerDto.UserName);
            if (user is not null)
                return BadRequest(ApiResponse<object>.Fail("UserName already exists"));

            user = mapper.Map<AppUser>(registerDto);
            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<object>.Fail(errors));
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

            return Ok(ApiResponse<object>.Ok(new { confirmLink }, "User registered successfully"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.Username)
                       ?? await userManager.FindByEmailAsync(loginDto.Username);

            if (user is null)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid username or password"));

            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid username or password"));

            if (!user.EmailConfirmed)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Please confirm your email."));

            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);
            var refreshToken = Guid.NewGuid().ToString();
            await userManager.SetAuthenticationTokenAsync(user, "ApiApp", "RefreshToken", refreshToken);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken
            };

            return Ok(ApiResponse<AuthResponseDto>.Ok(authResponse, "Login successful"));
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.Identity?.Name;
            var fullname = User.FindFirstValue("Fullname");
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var data = new { userId, username, fullname, roles };
            return Ok(ApiResponse<object>.Ok(data));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Təhlükəsizlik üçün eyni mesaj qaytarılır (user var/yox bilinməsin)
                return Ok(ApiResponse<object>.Ok(null, "If the email exists in the system, a password reset token has been sent."));
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            return Ok(ApiResponse<object>.Ok(new { Token = token }, "Token created successfully."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(ApiResponse<object>.Fail("Passwords do not match."));

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(ApiResponse<object>.Fail("User not found."));

            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<object>.Fail(errors));
            }

            return Ok(ApiResponse<object>.Ok(null, "Your password has been successfully reset."));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("User not found"));

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (FormatException)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid token format"));
            }

            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail("Invalid token"));

            return Ok(ApiResponse<object>.Ok(null, "Email confirmed successfully"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.Username)
                       ?? await userManager.FindByEmailAsync(dto.Username);
            if (user == null)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("User not found."));

            var savedRefreshToken = await userManager.GetAuthenticationTokenAsync(user, "ApiApp", "RefreshToken");
            if (savedRefreshToken != dto.RefreshToken)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Unacceptable refresh token."));

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = jwtService.GenerateToken(user, roles);
            var newRefreshToken = Guid.NewGuid().ToString();

            await userManager.SetAuthenticationTokenAsync(user, "ApiApp", "RefreshToken", newRefreshToken);

            var authResponse = new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return Ok(ApiResponse<AuthResponseDto>.Ok(authResponse, "Token refreshed successfully"));
        }
    }
}
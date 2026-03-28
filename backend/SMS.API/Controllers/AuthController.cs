using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Common;
using SMS.Application.Features.Auth.DTOs;
using SMS.Application.Interfaces;
using SMS.Infrastructure.Services;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = new List<string> { "Username and password are required" },
                    StatusCode = 400
                });
            }

            // Wire up cookie operations
            if (_authService is AuthService authService)
            {
                var refreshTokenFromCookie = Request.Cookies["refreshToken"] ?? "";
                authService.SetCookieActions(
                    refreshTokenFromCookie,
                    (token, expires) =>
                    {
                        Response.Cookies.Append("refreshToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = expires
                        });
                    },
                    () =>
                    {
                        Response.Cookies.Delete("refreshToken");
                    });
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var result = await _authService.RequestPasswordResetAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"] ?? "";

            if (_authService is AuthService authService)
            {
                authService.SetCookieActions(
                    refreshToken,
                    (token, expires) =>
                    {
                        Response.Cookies.Append("refreshToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = expires
                        });
                    },
                    () =>
                    {
                        Response.Cookies.Delete("refreshToken");
                    });
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"] ?? "";

            if (_authService is AuthService authService)
            {
                authService.SetCookieActions(
                    refreshToken,
                    (token, expires) => { },
                    () =>
                    {
                        Response.Cookies.Delete("refreshToken");
                    });
            }

            var result = await _authService.LogoutAsync(refreshToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = 401
                });
            }

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim != null && Guid.TryParse(claim.Value, out var id))
            {
                return id;
            }
            return Guid.Empty;
        }
    }
}

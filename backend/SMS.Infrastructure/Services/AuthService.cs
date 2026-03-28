using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SMS.Application.Common;
using SMS.Application.Features.Auth.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private string _refreshTokenFromCookie = string.Empty;
        private Action<string, DateTime>? _setRefreshTokenCookieAction;
        private Action? _clearRefreshTokenCookieAction;

        public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
            {
                return new BaseResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password",
                    StatusCode = 401
                };
            }

            if (!user.IsActive)
            {
                return new BaseResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Account is deactivated",
                    StatusCode = 401
                };
            }

            var passwordValid = VerifyPassword(dto.Password, user.PasswordHash);
            if (!passwordValid)
            {
                return new BaseResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password",
                    StatusCode = 401
                };
            }

            var accessToken = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

            var refreshToken = GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            SetRefreshTokenCookie(refreshToken, refreshTokenEntity.ExpiresAt);

            return new BaseResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                StatusCode = 200,
                Data = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    ExpiresAt = expiresAt,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Role = user.Role.ToString()
                    }
                }
            };
        }

        public async Task<BaseResponse<RefreshResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                ClearRefreshTokenCookie();
                return new BaseResponse<RefreshResponseDto>
                {
                    Success = false,
                    Message = "Refresh token is required",
                    StatusCode = 401
                };
            }

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                ClearRefreshTokenCookie();
                return new BaseResponse<RefreshResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token",
                    StatusCode = 401
                };
            }

            if (storedToken.RevokedAt != null)
            {
                await RevokeAllActiveRefreshTokensAsync(storedToken.UserId);
                ClearRefreshTokenCookie();
                return new BaseResponse<RefreshResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token",
                    StatusCode = 401
                };
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                ClearRefreshTokenCookie();
                return new BaseResponse<RefreshResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token",
                    StatusCode = 401
                };
            }

            if (!storedToken.User.IsActive)
            {
                ClearRefreshTokenCookie();
                return new BaseResponse<RefreshResponseDto>
                {
                    Success = false,
                    Message = "Account is deactivated",
                    StatusCode = 401
                };
            }

            // Revoke old token
            storedToken.RevokedAt = DateTime.UtcNow;

            // Create new refresh token
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = storedToken.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            var accessToken = GenerateJwtToken(storedToken.User);
            var expiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

            SetRefreshTokenCookie(newRefreshToken, newRefreshTokenEntity.ExpiresAt);

            return new BaseResponse<RefreshResponseDto>
            {
                Success = true,
                Message = "Token refreshed",
                StatusCode = 200,
                Data = new RefreshResponseDto
                {
                    AccessToken = accessToken,
                    ExpiresAt = expiresAt
                }
            };
        }

        private async Task RevokeAllActiveRefreshTokensAsync(Guid userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt >= DateTime.UtcNow)
                .ToListAsync();

            if (activeTokens.Count == 0)
            {
                return;
            }

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<BaseResponse<object>> LogoutAsync(string refreshToken)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken != null)
                {
                    storedToken.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            ClearRefreshTokenCookie();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Logged out successfully",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            var passwordValid = VerifyPassword(dto.CurrentPassword, user.PasswordHash);
            if (!passwordValid)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Current password is incorrect",
                    StatusCode = 400,
                    Errors = new List<string> { "Current password is incorrect" }
                };
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "New password and confirmation do not match",
                    StatusCode = 400,
                    Errors = new List<string> { "New password and confirmation do not match" }
                };
            }

            var passwordValidationErrors = ValidatePasswordRules(dto.NewPassword);
            if (passwordValidationErrors.Count > 0)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    StatusCode = 400,
                    Errors = passwordValidationErrors
                };
            }

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Password changed successfully",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<object>> RequestPasswordResetAsync(ForgotPasswordRequestDto dto)
        {
            var identifier = (dto.Email ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(identifier))
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Email or username is required.",
                    StatusCode = 400,
                    Errors = new List<string> { "Email or username is required." }
                };
            }

            var normalized = identifier.ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.IsActive &&
                ((u.Email != null && u.Email.ToLower() == normalized)
                 || (u.Username != null && u.Username.ToLower() == normalized)));

            string? targetEmail = user?.Email;

            if (user == null)
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.ParentContact)
                    .Where(s => s.User.IsActive
                        && s.ParentContact != null
                        && s.ParentContact.Email.ToLower() == normalized)
                    .FirstOrDefaultAsync();

                if (student != null)
                {
                    user = student.User;
                    targetEmail = student.ParentContact!.Email;
                }
            }

            if (user != null && !string.IsNullOrWhiteSpace(targetEmail))
            {
                var token = GeneratePasswordResetToken(user);
                var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://localhost:7056";
                var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

                await _emailService.TrySendEmailAsync(
                    targetEmail,
                    "SMS Password Reset",
                    $"Use this link to reset your password: {resetLink}");
            }

            return new BaseResponse<object>
            {
                Success = true,
                Message = "If the account exists, a password reset link will be sent. Please check your spam folder.",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<object>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "New password and confirmation do not match",
                    StatusCode = 400,
                    Errors = new List<string> { "New password and confirmation do not match" }
                };
            }

            var passwordValidationErrors = ValidatePasswordRules(dto.NewPassword);
            if (passwordValidationErrors.Count > 0)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    StatusCode = 400,
                    Errors = passwordValidationErrors
                };
            }

            var userId = ValidatePasswordResetToken(dto.Token);
            if (userId == Guid.Empty)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Invalid or expired reset token",
                    StatusCode = 400,
                    Errors = new List<string> { "Invalid or expired reset token" }
                };
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Password recovered successfully",
                StatusCode = 200
            };
        }

        public string GetRefreshTokenFromCookie()
        {
            return _refreshTokenFromCookie;
        }

        public void SetRefreshTokenCookie(string token, DateTime expires)
        {
            _setRefreshTokenCookieAction?.Invoke(token, expires);
        }

        public void ClearRefreshTokenCookie()
        {
            _clearRefreshTokenCookieAction?.Invoke();
        }

        // These are called by the controller to wire up cookie operations
        public void SetCookieActions(
            string refreshTokenFromCookie,
            Action<string, DateTime> setCookieAction,
            Action clearCookieAction)
        {
            _refreshTokenFromCookie = refreshTokenFromCookie;
            _setRefreshTokenCookieAction = setCookieAction;
            _clearRefreshTokenCookieAction = clearCookieAction;
        }

        private string GenerateJwtToken(User user)
        {
            var secret = _configuration["Jwt:Secret"] ?? "SuperSecretKeyThatIsLongEnoughForHmacSha256!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "SMS",
                audience: _configuration["Jwt:Audience"] ?? "SMS",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }

        private static List<string> ValidatePasswordRules(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                errors.Add("Password must be at least 8 characters.");
            if (!password.Any(char.IsUpper))
                errors.Add("Password must include at least one uppercase letter.");
            if (!password.Any(char.IsDigit))
                errors.Add("Password must include at least one number.");

            return errors;
        }

        private string GeneratePasswordResetToken(User user)
        {
            var secret = _configuration["Jwt:Secret"] ?? "SuperSecretKeyThatIsLongEnoughForHmacSha256!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("purpose", "password-reset")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "SMS",
                audience: _configuration["Jwt:Audience"] ?? "SMS",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Guid ValidatePasswordResetToken(string token)
        {
            try
            {
                var secret = _configuration["Jwt:Secret"] ?? "SuperSecretKeyThatIsLongEnoughForHmacSha256!";
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "SMS",
                    ValidAudience = _configuration["Jwt:Audience"] ?? "SMS",
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, tokenValidationParameters, out _);

                var purpose = principal.FindFirst("purpose")?.Value;
                if (purpose != "password-reset") return Guid.Empty;

                var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(idClaim, out var userId) ? userId : Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }
}

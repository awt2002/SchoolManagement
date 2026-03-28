using SMS.Application.Common;
using SMS.Application.Features.Auth.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<BaseResponse<RefreshResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<BaseResponse<object>> LogoutAsync(string refreshToken);
        Task<BaseResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<BaseResponse<object>> RequestPasswordResetAsync(ForgotPasswordRequestDto dto);
        Task<BaseResponse<object>> ResetPasswordAsync(ResetPasswordDto dto);
        string GetRefreshTokenFromCookie();
        void SetRefreshTokenCookie(string token, DateTime expires);
        void ClearRefreshTokenCookie();
    }
}

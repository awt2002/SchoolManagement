using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Auth.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class AuthHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;
        private readonly JwtAuthStateProvider _authStateProvider;

        public AuthHttpService(HttpClient http, TokenService tokenService, JwtAuthStateProvider authStateProvider)
        {
            _http = http;
            _tokenService = tokenService;
            _authStateProvider = authStateProvider;
        }

        public async Task<BaseResponse<LoginResponseDto>?> LoginAsync(LoginRequestDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/login", dto);
            var result = await response.Content.ReadFromJsonAsync<BaseResponse<LoginResponseDto>>();

            if (result != null && result.Success && result.Data != null)
            {
                await _tokenService.SetTokenAsync(result.Data.AccessToken, result.Data.ExpiresAt);
                _authStateProvider.NotifyAuthenticationStateChanged();
            }

            return result;
        }

        public async Task LogoutAsync()
        {
            try
            {
                SetAuthHeader();
                await _http.PostAsync("api/v1/auth/logout", null);
            }
            catch { }

            await _tokenService.ClearTokenAsync();
            _authStateProvider.NotifyAuthenticationStateChanged();
        }

        public async Task<bool> TryRefreshTokenAsync()
        {
            try
            {
                var response = await _http.PostAsync("api/v1/auth/refresh", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BaseResponse<RefreshResponseDto>>();
                    if (result?.Data != null)
                    {
                        await _tokenService.SetTokenAsync(result.Data.AccessToken, result.Data.ExpiresAt);
                        _authStateProvider.NotifyAuthenticationStateChanged();
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public async Task<BaseResponse<object>?> ChangePasswordAsync(ChangePasswordDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/auth/change-password", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<object>?> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/forgot-password", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<object>?> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/reset-password", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SMS.Application.Features.Auth.DTOs;
using SMS.Application.Features.Students.DTOs;
using SMS.Blazor.Auth;
using SMS.Blazor.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SMS.Blazor.Pages
{
    public partial class Profile
    {
        [Inject]
        private StudentHttpService StudentService { get; set; } = null!;

        [Inject]
        private AuthHttpService AuthService { get; set; } = null!;

        [Inject]
        private TokenService TokenService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private string _username = string.Empty;
        private StudentDetailDto? _studentDetail;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(TokenService.AccessToken);

                var usernameClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == ClaimTypes.Name);
                _username = usernameClaim?.Value ?? "Unknown";

                var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
                var role = roleClaim?.Value;

                if (role == "Student")
                {
                    var result = await StudentService.GetMyProfileAsync();
                    _studentDetail = result?.Data;
                }
            }
            catch { }
        }

        private async Task ChangePassword()
        {
            if (string.IsNullOrEmpty(_currentPassword) || string.IsNullOrEmpty(_newPassword) || string.IsNullOrEmpty(_confirmNewPassword))
            {
                Snackbar.Add("Please fill in all password fields", Severity.Warning);
                return;
            }

            if (_newPassword != _confirmNewPassword)
            {
                Snackbar.Add("New password and confirmation do not match", Severity.Warning);
                return;
            }

            var dto = new ChangePasswordDto
            {
                CurrentPassword = _currentPassword,
                NewPassword = _newPassword,
                ConfirmNewPassword = _confirmNewPassword
            };

            var result = await AuthService.ChangePasswordAsync(dto);

            if (result?.Success == true)
            {
                Snackbar.Add("Password changed successfully!", Severity.Success);
                _currentPassword = string.Empty;
                _newPassword = string.Empty;
                _confirmNewPassword = string.Empty;
            }
            else
            {
                var message = result?.Message ?? "Failed to change password";
                Snackbar.Add(message, Severity.Error);
            }
        }
    }
}

using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Dashboard.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class DashboardHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public DashboardHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<AdminDashboardDto>?> GetAdminDashboardAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<AdminDashboardDto>>("api/v1/dashboard");
        }

        public async Task<BaseResponse<TeacherDashboardDto>?> GetTeacherDashboardAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<TeacherDashboardDto>>("api/v1/dashboard");
        }

        public async Task<BaseResponse<StudentDashboardDto>?> GetStudentDashboardAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<StudentDashboardDto>>("api/v1/dashboard");
        }
    }
}

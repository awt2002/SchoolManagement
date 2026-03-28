using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class TeacherHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public TeacherHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<PagedResponse<TeacherDto>?> GetAllAsync(int page = 1, int pageSize = 10, string? search = null, bool includeInactive = false, bool inactiveOnly = false)
        {
            SetAuthHeader();
            var url = $"api/v1/teachers?page={page}&pageSize={pageSize}&includeInactive={includeInactive}";
            if (inactiveOnly) url += "&inactiveOnly=true";
            if (!string.IsNullOrEmpty(search)) url += $"&search={search}";
            return await _http.GetFromJsonAsync<PagedResponse<TeacherDto>>(url);
        }

        public async Task<BaseResponse<TeacherDto>?> GetByIdAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<TeacherDto>>($"api/v1/teachers/{id}");
        }

        public async Task<BaseResponse<TeacherDto>?> GetMeAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<TeacherDto>>("api/v1/teachers/me");
        }

        public async Task<BaseResponse<TeacherDto>?> CreateAsync(CreateTeacherDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/teachers", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<TeacherDto>>();
        }

        public async Task<BaseResponse<TeacherDto>?> UpdateAsync(Guid id, UpdateTeacherDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/teachers/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<TeacherDto>>();
        }

        public async Task<BaseResponse<object>?> DeleteAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/teachers/{id}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<object>?> ReactivateAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.PutAsync($"api/v1/teachers/{id}/reactivate", null);
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }
    }
}

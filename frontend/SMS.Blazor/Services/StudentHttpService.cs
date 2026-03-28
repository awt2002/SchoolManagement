using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class StudentHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public StudentHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<StudentDetailDto>?> GetMyProfileAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<StudentDetailDto>>("api/v1/students/me");
        }

        public async Task<PagedResponse<StudentSummaryDto>?> GetAllAsync(
            int page = 1, int pageSize = 10, string? search = null,
            int? gradeLevel = null, Guid? classId = null, bool includeInactive = false, bool inactiveOnly = false)
        {
            SetAuthHeader();
            var url = $"api/v1/students?page={page}&pageSize={pageSize}&includeInactive={includeInactive}";
            if (inactiveOnly) url += "&inactiveOnly=true";
            if (!string.IsNullOrEmpty(search)) url += $"&search={search}";
            if (gradeLevel.HasValue) url += $"&gradeLevel={gradeLevel}";
            if (classId.HasValue) url += $"&classId={classId}";
            return await _http.GetFromJsonAsync<PagedResponse<StudentSummaryDto>>(url);
        }

        public async Task<BaseResponse<StudentDetailDto>?> GetByIdAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<StudentDetailDto>>($"api/v1/students/{id}");
        }

        public async Task<BaseResponse<StudentDetailDto>?> CreateAsync(CreateStudentDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/students", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<StudentDetailDto>>();
        }

        public async Task<BaseResponse<StudentDetailDto>?> UpdateAsync(Guid id, UpdateStudentDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/students/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<StudentDetailDto>>();
        }

        public async Task<BaseResponse<object>?> DeleteAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/students/{id}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<object>?> ReactivateAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.PutAsync($"api/v1/students/{id}/reactivate", null);
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<object>?> UploadPhotoAsync(Guid id, MultipartFormDataContent content)
        {
            SetAuthHeader();
            var response = await _http.PostAsync($"api/v1/students/{id}/photo", content);
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<List<EnrollmentDto>>?> GetEnrollmentsAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<List<EnrollmentDto>>>($"api/v1/students/{id}/enrollments");
        }
    }
}

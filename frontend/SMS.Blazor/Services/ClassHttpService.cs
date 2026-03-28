using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Classes.DTOs;
using SMS.Application.Features.Students.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class ClassHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public ClassHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<List<ClassDto>>?> GetAllAsync(Guid? academicYearId = null, Guid? teacherId = null)
        {
            SetAuthHeader();
            var url = "api/v1/classes?";
            if (academicYearId.HasValue) url += $"academicYearId={academicYearId}&";
            if (teacherId.HasValue) url += $"teacherId={teacherId}&";
            return await _http.GetFromJsonAsync<BaseResponse<List<ClassDto>>>(url);
        }

        public async Task<BaseResponse<ClassDto>?> CreateAsync(CreateClassDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/classes", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<ClassDto>>();
        }

        public async Task<BaseResponse<ClassDetailDto>?> GetByIdAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<ClassDetailDto>>($"api/v1/classes/{id}");
        }

        public async Task<BaseResponse<ClassDto>?> UpdateAsync(Guid id, UpdateClassDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/classes/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<ClassDto>>();
        }

        public async Task<BaseResponse<object>?> DeleteAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/classes/{id}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<EnrollmentDto>?> EnrollStudentAsync(Guid classId, EnrollStudentDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync($"api/v1/classes/{classId}/enroll", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<EnrollmentDto>>();
        }

        public async Task<BaseResponse<object>?> RemoveStudentAsync(Guid classId, Guid studentId)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/classes/{classId}/enroll/{studentId}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }
    }
}

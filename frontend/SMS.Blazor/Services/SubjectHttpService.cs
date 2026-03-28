using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Subjects.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class SubjectHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public SubjectHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<List<SubjectDto>>?> GetAllAsync(Guid? classId = null)
        {
            SetAuthHeader();
            var url = "api/v1/subjects";
            if (classId.HasValue) url += $"?classId={classId}";
            return await _http.GetFromJsonAsync<BaseResponse<List<SubjectDto>>>(url);
        }

        public async Task<BaseResponse<SubjectDto>?> GetByIdAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<SubjectDto>>($"api/v1/subjects/{id}");
        }

        public async Task<BaseResponse<SubjectDto>?> CreateAsync(CreateSubjectDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/subjects", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<SubjectDto>>();
        }

        public async Task<BaseResponse<SubjectDto>?> UpdateAsync(Guid id, UpdateSubjectDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/subjects/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<SubjectDto>>();
        }

        public async Task<BaseResponse<object>?> DeleteAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/subjects/{id}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<List<GradeCategoryDto>>?> GetCategoriesAsync(Guid subjectId)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<List<GradeCategoryDto>>>($"api/v1/subjects/{subjectId}/categories");
        }

        public async Task<BaseResponse<GradeCategoryDto>?> CreateCategoryAsync(Guid subjectId, CreateGradeCategoryDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync($"api/v1/subjects/{subjectId}/categories", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<GradeCategoryDto>>();
        }

        public async Task<BaseResponse<GradeCategoryDto>?> UpdateCategoryAsync(Guid subjectId, Guid id, UpdateGradeCategoryDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/subjects/{subjectId}/categories/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<GradeCategoryDto>>();
        }

        public async Task<BaseResponse<object>?> DeleteCategoryAsync(Guid subjectId, Guid id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/subjects/{subjectId}/categories/{id}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }
    }
}

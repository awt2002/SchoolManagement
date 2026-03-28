using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Exams.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class ExamHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public ExamHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<List<ExamDto>>?> GetAllAsync(Guid? subjectId = null, Guid? classId = null)
        {
            SetAuthHeader();
            var url = "api/v1/exams?";
            if (subjectId.HasValue) url += $"subjectId={subjectId}&";
            if (classId.HasValue) url += $"classId={classId}&";
            return await _http.GetFromJsonAsync<BaseResponse<List<ExamDto>>>(url);
        }

        public async Task<BaseResponse<ExamDto>?> CreateAsync(CreateExamDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/exams", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<ExamDto>>();
        }

        public async Task<BaseResponse<ExamDto>?> GetByIdAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<ExamDto>>($"api/v1/exams/{id}");
        }

        public async Task<BaseResponse<ExamDto>?> UpdateAsync(Guid id, UpdateExamDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/exams/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<ExamDto>>();
        }

        public async Task<BaseResponse<List<ExamResultDto>>?> GetResultsAsync(Guid examId)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<List<ExamResultDto>>>($"api/v1/exams/{examId}/results");
        }

        public async Task<BaseResponse<List<ExamResultDto>>?> CreateResultsAsync(Guid examId, BulkExamResultDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync($"api/v1/exams/{examId}/results", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<List<ExamResultDto>>>();
        }

        public async Task<BaseResponse<ExamResultDetailDto>?> GetStudentResultAsync(Guid examId, Guid studentId)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<ExamResultDetailDto>>($"api/v1/exams/{examId}/results/{studentId}");
        }
    }
}

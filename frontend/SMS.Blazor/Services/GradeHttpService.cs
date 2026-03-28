using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Grades.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class GradeHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public GradeHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<List<GradeDto>>?> GetAllAsync(Guid? subjectId = null, Guid? studentId = null, Guid? academicYearId = null)
        {
            SetAuthHeader();
            var url = "api/v1/grades?";
            if (subjectId.HasValue) url += $"subjectId={subjectId}&";
            if (studentId.HasValue) url += $"studentId={studentId}&";
            if (academicYearId.HasValue) url += $"academicYearId={academicYearId}&";
            return await _http.GetFromJsonAsync<BaseResponse<List<GradeDto>>>(url);
        }

        public async Task<BaseResponse<GradeDto>?> CreateOrUpdateAsync(CreateGradeDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/grades", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<GradeDto>>();
        }

        public async Task<BaseResponse<GradeSummaryDto>?> GetSummaryAsync(Guid? studentId = null, Guid? classId = null, Guid? academicYearId = null)
        {
            SetAuthHeader();
            var url = "api/v1/grades/summary?";
            if (studentId.HasValue) url += $"studentId={studentId}&";
            if (classId.HasValue) url += $"classId={classId}&";
            if (academicYearId.HasValue) url += $"academicYearId={academicYearId}&";
            return await _http.GetFromJsonAsync<BaseResponse<GradeSummaryDto>>(url);
        }

        public async Task<PagedResponse<GradeAuditLogDto>?> GetAuditLogAsync(Guid? studentId = null, Guid? subjectId = null, int page = 1, int pageSize = 10)
        {
            SetAuthHeader();
            var url = $"api/v1/grades/audit?page={page}&pageSize={pageSize}";
            if (studentId.HasValue) url += $"&studentId={studentId}";
            if (subjectId.HasValue) url += $"&subjectId={subjectId}";
            return await _http.GetFromJsonAsync<PagedResponse<GradeAuditLogDto>>(url);
        }
    }
}

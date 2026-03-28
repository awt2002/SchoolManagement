using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class AttendanceHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public AttendanceHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<List<AttendanceRecordDto>>?> GetAllAsync(Guid? classId = null, Guid? studentId = null, DateOnly? from = null, DateOnly? to = null)
        {
            SetAuthHeader();
            var url = "api/v1/attendance?";
            if (classId.HasValue) url += $"classId={classId}&";
            if (studentId.HasValue) url += $"studentId={studentId}&";
            if (from.HasValue) url += $"from={from.Value:yyyy-MM-dd}&";
            if (to.HasValue) url += $"to={to.Value:yyyy-MM-dd}&";
            return await _http.GetFromJsonAsync<BaseResponse<List<AttendanceRecordDto>>>(url);
        }

        public async Task<BaseResponse<AttendanceRecordDto>?> CreateAsync(CreateAttendanceDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/attendance", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<AttendanceRecordDto>>();
        }

        public async Task<BaseResponse<object>?> DeleteAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/v1/attendance/{id}");
            return await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        }

        public async Task<BaseResponse<AttendanceSummaryDto>?> GetStudentSummaryAsync(Guid studentId, Guid? academicYearId = null)
        {
            SetAuthHeader();
            var url = $"api/v1/attendance/student/{studentId}/summary";
            if (academicYearId.HasValue) url += $"?academicYearId={academicYearId}";
            return await _http.GetFromJsonAsync<BaseResponse<AttendanceSummaryDto>>(url);
        }
    }
}

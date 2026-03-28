using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.AcademicYears.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class AcademicYearHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public AcademicYearHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<BaseResponse<List<AcademicYearDto>>?> GetAllAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<List<AcademicYearDto>>>("api/v1/academic-years");
        }

        public async Task<BaseResponse<AcademicYearDto>?> CreateAsync(CreateAcademicYearDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/academic-years", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<AcademicYearDto>>();
        }

        public async Task<BaseResponse<AcademicYearDto>?> UpdateAsync(Guid id, UpdateAcademicYearDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/v1/academic-years/{id}", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<AcademicYearDto>>();
        }

        public async Task<BaseResponse<AcademicYearDto>?> ActivateAsync(Guid id)
        {
            SetAuthHeader();
            var response = await _http.PutAsync($"api/v1/academic-years/{id}/activate", null);
            return await response.Content.ReadFromJsonAsync<BaseResponse<AcademicYearDto>>();
        }
    }
}

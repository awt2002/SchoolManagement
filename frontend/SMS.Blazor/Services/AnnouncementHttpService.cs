using System.Net.Http.Json;
using SMS.Application.Common;
using SMS.Application.Features.Announcements.DTOs;
using SMS.Blazor.Auth;

namespace SMS.Blazor.Services
{
    public class AnnouncementHttpService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;

        public AnnouncementHttpService(HttpClient http, TokenService tokenService)
        {
            _http = http;
            _tokenService = tokenService;
        }

        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);
        }

        public async Task<PagedResponse<AnnouncementDto>?> GetAllAsync(int page = 1, int pageSize = 10)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<PagedResponse<AnnouncementDto>>($"api/v1/announcements?page={page}&pageSize={pageSize}");
        }

        public async Task<BaseResponse<AnnouncementDto>?> CreateAsync(CreateAnnouncementDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/v1/announcements", dto);
            return await response.Content.ReadFromJsonAsync<BaseResponse<AnnouncementDto>>();
        }

        public async Task<BaseResponse<AnnouncementDto>?> GetByIdAsync(Guid id)
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<AnnouncementDto>>($"api/v1/announcements/{id}");
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            SetAuthHeader();
            await _http.PostAsync($"api/v1/announcements/{id}/read", null);
        }

        public async Task<BaseResponse<UnreadCountDto>?> GetUnreadCountAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<BaseResponse<UnreadCountDto>>("api/v1/announcements/unread-count");
        }
    }
}

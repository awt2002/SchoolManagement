using SMS.Application.Common;
using SMS.Application.Features.Announcements.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IAnnouncementService
    {
        Task<PagedResponse<AnnouncementDto>> GetAnnouncementsAsync(Guid userId, string userRole, Guid? classId, int page, int pageSize);
        Task<BaseResponse<AnnouncementDto>> CreateAnnouncementAsync(CreateAnnouncementDto dto, Guid authorId);
        Task<BaseResponse<AnnouncementDto>> GetAnnouncementByIdAsync(Guid id, Guid userId);
        Task<BaseResponse<object>> MarkAsReadAsync(Guid announcementId, Guid userId);
        Task<BaseResponse<UnreadCountDto>> GetUnreadCountAsync(Guid userId, string userRole, Guid? classId);
    }
}

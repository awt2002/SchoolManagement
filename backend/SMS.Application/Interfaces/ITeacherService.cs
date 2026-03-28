using SMS.Application.Common;
using SMS.Application.Features.Teachers.DTOs;

namespace SMS.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<PagedResponse<TeacherDto>> GetAllTeachersAsync(int page, int pageSize, string? search, bool includeInactive, bool inactiveOnly = false);
        Task<BaseResponse<TeacherDto>> GetTeacherByIdAsync(Guid id);
        Task<BaseResponse<TeacherDto>> GetTeacherByUserIdAsync(Guid userId);
        Task<BaseResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto);
        Task<BaseResponse<TeacherDto>> UpdateTeacherAsync(Guid id, UpdateTeacherDto dto);
        Task<BaseResponse<object>> DeleteTeacherAsync(Guid id);
        Task<BaseResponse<object>> ReactivateTeacherAsync(Guid id);
    }
}

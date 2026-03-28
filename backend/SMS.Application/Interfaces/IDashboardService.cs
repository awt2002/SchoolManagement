using SMS.Application.Common;
using SMS.Application.Features.Dashboard.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<BaseResponse<AdminDashboardDto>> GetAdminDashboardAsync(Guid userId);
        Task<BaseResponse<TeacherDashboardDto>> GetTeacherDashboardAsync(Guid userId);
        Task<BaseResponse<StudentDashboardDto>> GetStudentDashboardAsync(Guid userId);
    }
}

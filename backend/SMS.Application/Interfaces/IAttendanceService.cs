using SMS.Application.Common;
using SMS.Application.Features.Attendance.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<BaseResponse<List<AttendanceRecordDto>>> GetAttendanceAsync(Guid? classId, Guid? studentId, DateOnly? from, DateOnly? to);
        Task<BaseResponse<AttendanceRecordDto>> CreateAttendanceAsync(CreateAttendanceDto dto, Guid recordedByUserId);
        Task<BaseResponse<object>> DeleteAttendanceAsync(Guid id);
        Task<BaseResponse<AttendanceSummaryDto>> GetStudentSummaryAsync(Guid studentId, Guid? academicYearId);
    }
}

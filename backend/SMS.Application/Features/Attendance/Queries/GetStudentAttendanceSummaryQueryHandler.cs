using SMS.Application.Common;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Attendance.Queries
{
    public class GetStudentAttendanceSummaryQueryHandler
    {
        private readonly IAttendanceService _attendanceService;

        public GetStudentAttendanceSummaryQueryHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public Task<BaseResponse<AttendanceSummaryDto>> HandleAsync(GetStudentAttendanceSummaryQuery query)
        {
            return _attendanceService.GetStudentSummaryAsync(query.StudentId, query.AcademicYearId);
        }
    }
}

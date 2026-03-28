using SMS.Application.Common;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Attendance.Queries
{
    public class GetAttendanceQueryHandler
    {
        private readonly IAttendanceService _attendanceService;

        public GetAttendanceQueryHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public Task<BaseResponse<List<AttendanceRecordDto>>> HandleAsync(GetAttendanceQuery query)
        {
            return _attendanceService.GetAttendanceAsync(query.ClassId, query.StudentId, query.From, query.To);
        }
    }
}

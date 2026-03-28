using SMS.Application.Common;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Attendance.Commands
{
    public class DeleteAttendanceCommandHandler
    {
        private readonly IAttendanceService _attendanceService;

        public DeleteAttendanceCommandHandler(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public Task<BaseResponse<object>> HandleAsync(DeleteAttendanceCommand command)
        {
            return _attendanceService.DeleteAttendanceAsync(command.Id);
        }
    }
}

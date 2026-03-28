using Moq;
using SMS.Application.Common;
using SMS.Application.Features.Attendance.Commands;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Features.Attendance.Queries;
using SMS.Application.Interfaces;
using Xunit;

namespace SMS.Application.Tests.Features.Attendance
{
    public class AttendanceHandlersTests
    {
        [Fact]
        public async Task GetAttendanceQueryHandler_ForwardsQueryToService()
        {
            var attendanceService = new Mock<IAttendanceService>();
            attendanceService
                .Setup(s => s.GetAttendanceAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(new BaseResponse<List<AttendanceRecordDto>>
                {
                    Success = true,
                    StatusCode = 200,
                    Data = new List<AttendanceRecordDto>()
                });

            var handler = new GetAttendanceQueryHandler(attendanceService.Object);
            var query = new GetAttendanceQuery
            {
                ClassId = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                From = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
                To = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var result = await handler.HandleAsync(query);

            Assert.True(result.Success);
            attendanceService.Verify(s => s.GetAttendanceAsync(query.ClassId, query.StudentId, query.From, query.To), Times.Once);
        }

        [Fact]
        public async Task DeleteAttendanceCommandHandler_ForwardsCommandToService()
        {
            var attendanceService = new Mock<IAttendanceService>();
            attendanceService
                .Setup(s => s.DeleteAttendanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new BaseResponse<object> { Success = true, StatusCode = 200 });

            var handler = new DeleteAttendanceCommandHandler(attendanceService.Object);
            var command = new DeleteAttendanceCommand { Id = Guid.NewGuid() };

            var result = await handler.HandleAsync(command);

            Assert.True(result.Success);
            attendanceService.Verify(s => s.DeleteAttendanceAsync(command.Id), Times.Once);
        }

        [Fact]
        public async Task GetStudentAttendanceSummaryQueryHandler_ForwardsQueryToService()
        {
            var attendanceService = new Mock<IAttendanceService>();
            attendanceService
                .Setup(s => s.GetStudentSummaryAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .ReturnsAsync(new BaseResponse<AttendanceSummaryDto>
                {
                    Success = true,
                    StatusCode = 200,
                    Data = new AttendanceSummaryDto { TotalAbsences = 0 }
                });

            var handler = new GetStudentAttendanceSummaryQueryHandler(attendanceService.Object);
            var query = new GetStudentAttendanceSummaryQuery
            {
                StudentId = Guid.NewGuid(),
                AcademicYearId = Guid.NewGuid()
            };

            var result = await handler.HandleAsync(query);

            Assert.True(result.Success);
            attendanceService.Verify(s => s.GetStudentSummaryAsync(query.StudentId, query.AcademicYearId), Times.Once);
        }
    }
}

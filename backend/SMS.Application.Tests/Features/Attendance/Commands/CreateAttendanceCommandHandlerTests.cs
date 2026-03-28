using FluentValidation;
using FluentValidation.Results;
using Moq;
using SMS.Application.Features.Attendance.Commands;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Interfaces;
using Xunit;

namespace SMS.Application.Tests.Features.Attendance.Commands
{
    public class CreateAttendanceCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WhenValidationFails_ReturnsBadRequestAndDoesNotCallService()
        {
            var attendanceService = new Mock<IAttendanceService>();
            var validator = new Mock<IValidator<CreateAttendanceDto>>();
            validator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateAttendanceDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("StudentId", "StudentId is required.")
                }));

            var handler = new CreateAttendanceCommandHandler(attendanceService.Object, validator.Object);
            var command = new CreateAttendanceCommand
            {
                StudentId = Guid.Empty,
                ClassId = Guid.NewGuid(),
                AbsenceDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var result = await handler.HandleAsync(command, Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("StudentId is required.", result.Errors!);
            attendanceService.Verify(s => s.CreateAttendanceAsync(It.IsAny<CreateAttendanceDto>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenValidationSucceeds_CallsServiceWithMappedDto()
        {
            var attendanceService = new Mock<IAttendanceService>();
            var validator = new Mock<IValidator<CreateAttendanceDto>>();
            validator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateAttendanceDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var expectedResponse = new SMS.Application.Common.BaseResponse<AttendanceRecordDto>
            {
                Success = true,
                StatusCode = 201,
                Message = "Attendance recorded",
                Data = new AttendanceRecordDto { Id = Guid.NewGuid() }
            };

            attendanceService
                .Setup(s => s.CreateAttendanceAsync(It.IsAny<CreateAttendanceDto>(), It.IsAny<Guid>()))
                .ReturnsAsync(expectedResponse);

            var handler = new CreateAttendanceCommandHandler(attendanceService.Object, validator.Object);
            var recordedBy = Guid.NewGuid();
            var command = new CreateAttendanceCommand
            {
                StudentId = Guid.NewGuid(),
                ClassId = Guid.NewGuid(),
                AbsenceDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var result = await handler.HandleAsync(command, recordedBy);

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            attendanceService.Verify(s => s.CreateAttendanceAsync(
                It.Is<CreateAttendanceDto>(d =>
                    d.StudentId == command.StudentId &&
                    d.ClassId == command.ClassId &&
                    d.AbsenceDate == command.AbsenceDate),
                recordedBy), Times.Once);
        }
    }
}

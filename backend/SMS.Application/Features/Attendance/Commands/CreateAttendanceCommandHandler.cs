using FluentValidation;
using SMS.Application.Common;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Attendance.Commands
{
    public class CreateAttendanceCommandHandler
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IValidator<CreateAttendanceDto> _validator;

        public CreateAttendanceCommandHandler(
            IAttendanceService attendanceService,
            IValidator<CreateAttendanceDto> validator)
        {
            _attendanceService = attendanceService;
            _validator = validator;
        }

        public async Task<BaseResponse<AttendanceRecordDto>> HandleAsync(CreateAttendanceCommand command, Guid recordedByUserId)
        {
            var dto = command.ToDto();
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return new BaseResponse<AttendanceRecordDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    StatusCode = 400,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
            }

            return await _attendanceService.CreateAttendanceAsync(dto, recordedByUserId);
        }
    }
}

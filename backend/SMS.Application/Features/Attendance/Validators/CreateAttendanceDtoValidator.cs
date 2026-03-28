using FluentValidation;
using SMS.Application.Features.Attendance.DTOs;

namespace SMS.Application.Features.Attendance.Validators
{
    public class CreateAttendanceDtoValidator : AbstractValidator<CreateAttendanceDto>
    {
        public CreateAttendanceDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .NotEmpty().WithMessage("StudentId is required.");

            RuleFor(x => x.ClassId)
                .NotEmpty().WithMessage("ClassId is required.");

            RuleFor(x => x.AbsenceDate)
                .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Absence date cannot be in the future.");
        }
    }
}

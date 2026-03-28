using SMS.Application.Features.Attendance.DTOs;

namespace SMS.Application.Features.Attendance.Commands
{
    public class CreateAttendanceCommand
    {
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
        public DateOnly AbsenceDate { get; set; }

        public CreateAttendanceDto ToDto()
        {
            return new CreateAttendanceDto
            {
                StudentId = StudentId,
                ClassId = ClassId,
                AbsenceDate = AbsenceDate
            };
        }
    }
}

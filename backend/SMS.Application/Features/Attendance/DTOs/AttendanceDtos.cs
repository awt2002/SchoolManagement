namespace SMS.Application.Features.Attendance.DTOs
{
    public class CreateAttendanceDto
    {
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
        public DateOnly AbsenceDate { get; set; }
    }

    public class AttendanceRecordDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateOnly AbsenceDate { get; set; }
        public string RecordedByName { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int TotalAbsences { get; set; }
        public List<DateOnly> AbsenceDates { get; set; } = new List<DateOnly>();
    }
}

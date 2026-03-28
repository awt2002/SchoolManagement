namespace SMS.Application.Features.Attendance.Queries
{
    public class GetAttendanceQuery
    {
        public Guid? ClassId { get; set; }
        public Guid? StudentId { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
}

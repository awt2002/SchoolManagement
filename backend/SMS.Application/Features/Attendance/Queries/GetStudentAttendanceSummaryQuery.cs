namespace SMS.Application.Features.Attendance.Queries
{
    public class GetStudentAttendanceSummaryQuery
    {
        public Guid StudentId { get; set; }
        public Guid? AcademicYearId { get; set; }
    }
}

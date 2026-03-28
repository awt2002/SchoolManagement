namespace SMS.Domain.Entities
{
    public class Class
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid AcademicYearId { get; set; }
        public Teacher? Teacher { get; set; }
        public AcademicYear AcademicYear { get; set; } = null!;
        public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public List<Subject> Subjects { get; set; } = new List<Subject>();
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}

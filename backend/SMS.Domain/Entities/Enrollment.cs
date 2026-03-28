namespace SMS.Domain.Entities
{
    public class Enrollment
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
        public Guid AcademicYearId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public AcademicYear AcademicYear { get; set; } = null!;
    }
}

namespace SMS.Domain.Entities
{
    public class AttendanceRecord
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
        public DateOnly AbsenceDate { get; set; }
        public Guid RecordedBy { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public User RecordedByUser { get; set; } = null!;
    }
}

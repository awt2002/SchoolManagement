namespace SMS.Domain.Entities
{
    public class ExamResult
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public decimal Score { get; set; }
        public Guid EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
        public Exam Exam { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public User EnteredByUser { get; set; } = null!;
    }
}

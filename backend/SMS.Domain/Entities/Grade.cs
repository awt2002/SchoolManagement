namespace SMS.Domain.Entities
{
    public class Grade
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid GradeCategoryId { get; set; }
        public decimal Score { get; set; }
        public Guid EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public Student Student { get; set; } = null!;
        public GradeCategory GradeCategory { get; set; } = null!;
        public User EnteredByUser { get; set; } = null!;
        public List<GradeAuditLog> AuditLogs { get; set; } = new List<GradeAuditLog>();

        public void UpdateScore(decimal newScore, Guid updatedBy)
        {
            if (updatedBy == Guid.Empty)
            {
                throw new ArgumentException("UpdatedBy is required", nameof(updatedBy));
            }

            if (newScore < 0 || newScore > 100)
            {
                throw new InvalidOperationException("Score must be between 0 and 100.");
            }

            Score = newScore;
            EnteredBy = updatedBy;
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}

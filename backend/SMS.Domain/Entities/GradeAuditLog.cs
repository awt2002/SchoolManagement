namespace SMS.Domain.Entities
{
    public class GradeAuditLog
    {
        public Guid Id { get; set; }
        public Guid GradeId { get; set; }
        public decimal OldScore { get; set; }
        public decimal NewScore { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Grade Grade { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}

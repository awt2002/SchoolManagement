namespace SMS.Domain.Entities
{
    public class Exam
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public decimal PassingThreshold { get; set; } = 50m;
        public Guid CreatedBy { get; set; }
        public Subject Subject { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public List<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}

namespace SMS.Application.Features.Exams.DTOs
{
    public class CreateExamDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public DateOnly ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public decimal PassingThreshold { get; set; } = 50m;
    }

    public class UpdateExamDto
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public decimal PassingThreshold { get; set; }
    }

    public class ExamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DateOnly ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public decimal PassingThreshold { get; set; }
    }

    public class ExamResultDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
    }

    public class ExamResultDetailDto
    {
        public decimal Score { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
        public decimal Average { get; set; }
        public decimal Highest { get; set; }
        public decimal Lowest { get; set; }
    }

    public class BulkExamResultDto
    {
        public List<ExamScoreEntryDto> Results { get; set; } = new List<ExamScoreEntryDto>();
    }

    public class ExamScoreEntryDto
    {
        public Guid StudentId { get; set; }
        public decimal Score { get; set; }
    }
}

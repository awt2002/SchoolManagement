namespace SMS.Application.Features.Grades.DTOs
{
    public class CreateGradeDto
    {
        public Guid StudentId { get; set; }
        public Guid GradeCategoryId { get; set; }
        public decimal Score { get; set; }
    }

    public class GradeDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid GradeCategoryId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal WeightedContribution { get; set; }
    }

    public class GradeSummaryDto
    {
        public List<SubjectAverageDto> SubjectAverages { get; set; } = new List<SubjectAverageDto>();
        public decimal Gpa { get; set; }
    }

    public class SubjectAverageDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public decimal WeightedAverage { get; set; }
    }

    public class GradeAuditLogDto
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal OldScore { get; set; }
        public decimal NewScore { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}

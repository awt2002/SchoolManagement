namespace SMS.Application.Features.Subjects.DTOs
{
    public class CreateGradeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class UpdateGradeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class GradeCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public Guid SubjectId { get; set; }
    }
}

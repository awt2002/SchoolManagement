namespace SMS.Application.Features.Subjects.DTOs
{
    public class CreateSubjectDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
    }

    public class UpdateSubjectDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class SubjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public List<GradeCategoryDto> GradeCategories { get; set; } = new List<GradeCategoryDto>();
    }
}

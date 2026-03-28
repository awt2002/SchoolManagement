namespace SMS.Domain.Entities
{
    public class Subject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public List<GradeCategory> GradeCategories { get; set; } = new List<GradeCategory>();
        public List<Exam> Exams { get; set; } = new List<Exam>();
    }
}

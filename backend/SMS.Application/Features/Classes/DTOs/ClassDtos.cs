namespace SMS.Application.Features.Classes.DTOs
{
    public class CreateClassDto
    {
        public string Name { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid AcademicYearId { get; set; }
    }

    public class UpdateClassDto
    {
        public string Name { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid AcademicYearId { get; set; }
    }

    public class ClassDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string? TeacherName { get; set; }
        public Guid? TeacherId { get; set; }
        public int StudentCount { get; set; }
        public Guid AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
    }

    public class ClassDetailDto : ClassDto
    {
        public List<ClassStudentDto> Students { get; set; } = new List<ClassStudentDto>();
        public List<ClassSubjectDto> Subjects { get; set; } = new List<ClassSubjectDto>();
    }

    public class ClassStudentDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class ClassSubjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class EnrollStudentDto
    {
        public Guid StudentId { get; set; }
    }
}

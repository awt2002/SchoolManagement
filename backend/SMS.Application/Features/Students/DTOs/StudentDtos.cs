namespace SMS.Application.Features.Students.DTOs
{
    public class CreateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public Guid? ClassId { get; set; }
        public Guid? AcademicYearId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
    }

    public class UpdateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
    }

    public class StudentSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public bool IsActive { get; set; }
    }

    public class StudentDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public int EnrollmentYear { get; set; }
        public bool IsActive { get; set; }
        public Guid? CurrentClassId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public List<EnrollmentDto> Enrollments { get; set; } = new List<EnrollmentDto>();
    }

    public class EnrollmentDto
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYearName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }
}

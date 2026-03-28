using SMS.Application.Features.Students.DTOs;

namespace SMS.Application.Features.Students.Commands
{
    public class CreateStudentCommand
    {
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public Guid? ClassId { get; set; }
        public Guid? AcademicYearId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;

        public CreateStudentDto ToDto()
        {
            return new CreateStudentDto
            {
                FullName = FullName,
                DateOfBirth = DateOfBirth,
                Address = Address,
                ClassId = ClassId,
                AcademicYearId = AcademicYearId,
                ParentName = ParentName,
                ParentEmail = ParentEmail,
                ParentPhone = ParentPhone
            };
        }
    }
}

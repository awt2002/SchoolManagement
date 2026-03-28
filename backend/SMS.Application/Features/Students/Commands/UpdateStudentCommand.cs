using SMS.Application.Features.Students.DTOs;

namespace SMS.Application.Features.Students.Commands
{
    public class UpdateStudentCommand
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ParentEmail { get; set; }

        public UpdateStudentDto ToDto()
        {
            return new UpdateStudentDto
            {
                FullName = FullName,
                ParentEmail = ParentEmail ?? string.Empty
            };
        }
    }
}

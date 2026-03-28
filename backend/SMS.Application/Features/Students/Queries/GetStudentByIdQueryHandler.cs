using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Students.Queries
{
    public class GetStudentByIdQueryHandler
    {
        private readonly IStudentService _studentService;

        public GetStudentByIdQueryHandler(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public Task<BaseResponse<StudentDetailDto>> HandleAsync(GetStudentByIdQuery query)
        {
            return _studentService.GetStudentByIdAsync(query.Id);
        }
    }
}

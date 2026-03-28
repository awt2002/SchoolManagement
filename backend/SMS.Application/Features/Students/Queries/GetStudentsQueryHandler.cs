using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Students.Queries
{
    public class GetStudentsQueryHandler
    {
        private readonly IStudentService _studentService;

        public GetStudentsQueryHandler(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public Task<PagedResponse<StudentSummaryDto>> HandleAsync(GetStudentsQuery query)
        {
            return _studentService.GetAllStudentsAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.GradeLevel,
                query.ClassId,
                query.AcademicYearId,
                query.IncludeInactive,
                query.InactiveOnly);
        }
    }
}

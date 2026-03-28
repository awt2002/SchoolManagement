using SMS.Application.Common;
using SMS.Application.Features.Grades.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IGradeService
    {
        Task<BaseResponse<List<GradeDto>>> GetGradesAsync(Guid? subjectId, Guid? studentId, Guid? academicYearId);
        Task<BaseResponse<GradeDto>> CreateOrUpdateGradeAsync(CreateGradeDto dto, Guid enteredByUserId);
        Task<BaseResponse<GradeSummaryDto>> GetGradeSummaryAsync(Guid? studentId, Guid? classId, Guid? academicYearId);
        Task<PagedResponse<GradeAuditLogDto>> GetAuditLogAsync(Guid? studentId, Guid? subjectId, DateOnly? from, DateOnly? to, int page, int pageSize);
    }
}

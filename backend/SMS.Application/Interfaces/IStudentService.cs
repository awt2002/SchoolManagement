using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResponse<StudentSummaryDto>> GetAllStudentsAsync(int page, int pageSize, string? search, int? gradeLevel, Guid? classId, Guid? academicYearId, bool includeInactive, bool inactiveOnly = false);
        Task<BaseResponse<StudentDetailDto>> GetStudentByIdAsync(Guid id);
        Task<BaseResponse<StudentDetailDto>> GetStudentByUserIdAsync(Guid userId);
        Task<BaseResponse<StudentDetailDto>> CreateStudentAsync(CreateStudentDto dto);
        Task<BaseResponse<StudentDetailDto>> UpdateStudentAsync(Guid id, UpdateStudentDto dto);
        Task<BaseResponse<object>> DeleteStudentAsync(Guid id);
        Task<BaseResponse<object>> ReactivateStudentAsync(Guid id);
        Task<BaseResponse<object>> UploadPhotoAsync(Guid id, string fileName, Stream fileStream);
        Task<BaseResponse<List<EnrollmentDto>>> GetEnrollmentsAsync(Guid studentId);
    }
}

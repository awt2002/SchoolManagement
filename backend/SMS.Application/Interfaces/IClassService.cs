using SMS.Application.Common;
using SMS.Application.Features.Classes.DTOs;
using SMS.Application.Features.Students.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IClassService
    {
        Task<BaseResponse<List<ClassDto>>> GetAllClassesAsync(Guid? academicYearId, Guid? teacherId);
        Task<BaseResponse<ClassDto>> CreateClassAsync(CreateClassDto dto);
        Task<BaseResponse<ClassDetailDto>> GetClassByIdAsync(Guid id);
        Task<BaseResponse<ClassDto>> UpdateClassAsync(Guid id, UpdateClassDto dto);
        Task<BaseResponse<object>> DeleteClassAsync(Guid id);
        Task<BaseResponse<EnrollmentDto>> EnrollStudentAsync(Guid classId, EnrollStudentDto dto);
        Task<BaseResponse<object>> RemoveStudentFromClassAsync(Guid classId, Guid studentId);
    }
}

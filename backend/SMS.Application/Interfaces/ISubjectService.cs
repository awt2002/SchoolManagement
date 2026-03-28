using SMS.Application.Common;
using SMS.Application.Features.Subjects.DTOs;

namespace SMS.Application.Interfaces
{
    public interface ISubjectService
    {
        Task<BaseResponse<List<SubjectDto>>> GetSubjectsAsync(Guid? classId);
        Task<BaseResponse<SubjectDto>> CreateSubjectAsync(CreateSubjectDto dto);
        Task<BaseResponse<SubjectDto>> GetSubjectByIdAsync(Guid id);
        Task<BaseResponse<SubjectDto>> UpdateSubjectAsync(Guid id, UpdateSubjectDto dto);
        Task<BaseResponse<object>> DeleteSubjectAsync(Guid id);
        Task<BaseResponse<List<GradeCategoryDto>>> GetCategoriesAsync(Guid subjectId);
        Task<BaseResponse<GradeCategoryDto>> CreateCategoryAsync(Guid subjectId, CreateGradeCategoryDto dto);
        Task<BaseResponse<GradeCategoryDto>> UpdateCategoryAsync(Guid subjectId, Guid categoryId, UpdateGradeCategoryDto dto);
        Task<BaseResponse<object>> DeleteCategoryAsync(Guid subjectId, Guid categoryId);
    }
}

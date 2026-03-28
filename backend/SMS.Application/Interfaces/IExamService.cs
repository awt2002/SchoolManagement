using SMS.Application.Common;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IExamService
    {
        Task<BaseResponse<List<ExamDto>>> GetExamsAsync(Guid? subjectId, Guid? classId, DateOnly? from, DateOnly? to);
        Task<BaseResponse<ExamDto>> CreateExamAsync(CreateExamDto dto, Guid createdByUserId);
        Task<BaseResponse<ExamDto>> GetExamByIdAsync(Guid id);
        Task<BaseResponse<ExamDto>> UpdateExamAsync(Guid id, UpdateExamDto dto);
        Task<BaseResponse<List<ExamResultDto>>> GetExamResultsAsync(Guid examId);
        Task<BaseResponse<List<ExamResultDto>>> CreateExamResultsAsync(Guid examId, BulkExamResultDto dto, Guid enteredByUserId);
        Task<BaseResponse<ExamResultDetailDto>> GetStudentExamResultAsync(Guid examId, Guid studentId);
    }
}

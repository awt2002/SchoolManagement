using SMS.Application.Common;
using SMS.Application.Features.AcademicYears.DTOs;

namespace SMS.Application.Interfaces
{
    public interface IAcademicYearService
    {
        Task<BaseResponse<List<AcademicYearDto>>> GetAllAsync();
        Task<BaseResponse<AcademicYearDto>> CreateAsync(CreateAcademicYearDto dto);
        Task<BaseResponse<AcademicYearDto>> UpdateAsync(Guid id, UpdateAcademicYearDto dto);
        Task<BaseResponse<AcademicYearDto>> ActivateAsync(Guid id);
    }
}

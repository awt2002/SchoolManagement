using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.AcademicYears.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly AppDbContext _context;

        public AcademicYearService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<List<AcademicYearDto>>> GetAllAsync()
        {
            var years = await _context.AcademicYears
                .OrderByDescending(a => a.StartDate)
                .Select(a => new AcademicYearDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return new BaseResponse<List<AcademicYearDto>>
            {
                Success = true,
                Message = "Academic years retrieved",
                StatusCode = 200,
                Data = years
            };
        }

        public async Task<BaseResponse<AcademicYearDto>> CreateAsync(CreateAcademicYearDto dto)
        {
            var academicYear = new AcademicYear
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = false
            };

            _context.AcademicYears.Add(academicYear);
            await _context.SaveChangesAsync();

            return new BaseResponse<AcademicYearDto>
            {
                Success = true,
                Message = "Academic year created",
                StatusCode = 201,
                Data = new AcademicYearDto
                {
                    Id = academicYear.Id,
                    Name = academicYear.Name,
                    StartDate = academicYear.StartDate,
                    EndDate = academicYear.EndDate,
                    IsActive = academicYear.IsActive
                }
            };
        }

        public async Task<BaseResponse<AcademicYearDto>> UpdateAsync(Guid id, UpdateAcademicYearDto dto)
        {
            var academicYear = await _context.AcademicYears.FindAsync(id);
            if (academicYear == null)
            {
                return new BaseResponse<AcademicYearDto>
                {
                    Success = false,
                    Message = "Academic year not found",
                    StatusCode = 404
                };
            }

            academicYear.Name = dto.Name;
            academicYear.StartDate = dto.StartDate;
            academicYear.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();

            return new BaseResponse<AcademicYearDto>
            {
                Success = true,
                Message = "Academic year updated",
                StatusCode = 200,
                Data = new AcademicYearDto
                {
                    Id = academicYear.Id,
                    Name = academicYear.Name,
                    StartDate = academicYear.StartDate,
                    EndDate = academicYear.EndDate,
                    IsActive = academicYear.IsActive
                }
            };
        }

        public async Task<BaseResponse<AcademicYearDto>> ActivateAsync(Guid id)
        {
            var academicYear = await _context.AcademicYears.FindAsync(id);
            if (academicYear == null)
            {
                return new BaseResponse<AcademicYearDto>
                {
                    Success = false,
                    Message = "Academic year not found",
                    StatusCode = 404
                };
            }

            // Deactivate all others
            var allYears = await _context.AcademicYears.ToListAsync();
            foreach (var year in allYears)
            {
                year.IsActive = false;
            }

            academicYear.IsActive = true;
            await _context.SaveChangesAsync();

            return new BaseResponse<AcademicYearDto>
            {
                Success = true,
                Message = "Academic year activated",
                StatusCode = 200,
                Data = new AcademicYearDto
                {
                    Id = academicYear.Id,
                    Name = academicYear.Name,
                    StartDate = academicYear.StartDate,
                    EndDate = academicYear.EndDate,
                    IsActive = academicYear.IsActive
                }
            };
        }
    }
}

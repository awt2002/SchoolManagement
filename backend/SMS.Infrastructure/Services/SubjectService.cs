using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Subjects.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly AppDbContext _context;

        public SubjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<List<SubjectDto>>> GetSubjectsAsync(Guid? classId)
        {
            var query = _context.Subjects
                .Include(s => s.GradeCategories)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(s => s.ClassId == classId.Value);
            }

            var subjects = await query
                .Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ClassId = s.ClassId,
                    GradeCategories = s.GradeCategories.Select(gc => new GradeCategoryDto
                    {
                        Id = gc.Id,
                        Name = gc.Name,
                        Weight = gc.Weight,
                        SubjectId = gc.SubjectId
                    }).ToList()
                })
                .ToListAsync();

            return new BaseResponse<List<SubjectDto>>
            {
                Success = true,
                Message = "Subjects retrieved",
                StatusCode = 200,
                Data = subjects
            };
        }

        public async Task<BaseResponse<SubjectDto>> CreateSubjectAsync(CreateSubjectDto dto)
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ClassId = dto.ClassId
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return new BaseResponse<SubjectDto>
            {
                Success = true,
                Message = "Subject created",
                StatusCode = 201,
                Data = new SubjectDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    ClassId = subject.ClassId,
                    GradeCategories = new List<GradeCategoryDto>()
                }
            };
        }

        public async Task<BaseResponse<SubjectDto>> GetSubjectByIdAsync(Guid id)
        {
            var subject = await _context.Subjects
                .Include(s => s.GradeCategories)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
            {
                return new BaseResponse<SubjectDto>
                {
                    Success = false,
                    Message = "Subject not found",
                    StatusCode = 404
                };
            }

            return new BaseResponse<SubjectDto>
            {
                Success = true,
                Message = "Subject retrieved",
                StatusCode = 200,
                Data = new SubjectDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    ClassId = subject.ClassId,
                    GradeCategories = subject.GradeCategories.Select(gc => new GradeCategoryDto
                    {
                        Id = gc.Id,
                        Name = gc.Name,
                        Weight = gc.Weight,
                        SubjectId = gc.SubjectId
                    }).ToList()
                }
            };
        }

        public async Task<BaseResponse<SubjectDto>> UpdateSubjectAsync(Guid id, UpdateSubjectDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return new BaseResponse<SubjectDto>
                {
                    Success = false,
                    Message = "Subject not found",
                    StatusCode = 404
                };
            }

            subject.Name = dto.Name;
            await _context.SaveChangesAsync();

            return await GetSubjectByIdAsync(id);
        }

        public async Task<BaseResponse<object>> DeleteSubjectAsync(Guid id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Subject not found",
                    StatusCode = 404
                };
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Subject deleted",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<List<GradeCategoryDto>>> GetCategoriesAsync(Guid subjectId)
        {
            var categories = await _context.GradeCategories
                .Where(gc => gc.SubjectId == subjectId)
                .Select(gc => new GradeCategoryDto
                {
                    Id = gc.Id,
                    Name = gc.Name,
                    Weight = gc.Weight,
                    SubjectId = gc.SubjectId
                })
                .ToListAsync();

            return new BaseResponse<List<GradeCategoryDto>>
            {
                Success = true,
                Message = "Categories retrieved",
                StatusCode = 200,
                Data = categories
            };
        }

        public async Task<BaseResponse<GradeCategoryDto>> CreateCategoryAsync(Guid subjectId, CreateGradeCategoryDto dto)
        {
            var subject = await _context.Subjects
                .Include(s => s.GradeCategories)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject == null)
            {
                return new BaseResponse<GradeCategoryDto>
                {
                    Success = false,
                    Message = "Subject not found",
                    StatusCode = 404
                };
            }

            // Check if weights would sum to more than 100
            var currentWeightSum = subject.GradeCategories.Sum(gc => gc.Weight);
            if (currentWeightSum + dto.Weight > 100)
            {
                return new BaseResponse<GradeCategoryDto>
                {
                    Success = false,
                    Message = "Grade category weights must total exactly 100%",
                    StatusCode = 400,
                    Errors = new List<string> { "Grade category weights must total exactly 100%." }
                };
            }

            var category = new GradeCategory
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Name = dto.Name,
                Weight = dto.Weight
            };

            _context.GradeCategories.Add(category);
            await _context.SaveChangesAsync();

            return new BaseResponse<GradeCategoryDto>
            {
                Success = true,
                Message = "Category created",
                StatusCode = 201,
                Data = new GradeCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Weight = category.Weight,
                    SubjectId = category.SubjectId
                }
            };
        }

        public async Task<BaseResponse<GradeCategoryDto>> UpdateCategoryAsync(
            Guid subjectId, Guid categoryId, UpdateGradeCategoryDto dto)
        {
            var category = await _context.GradeCategories
                .FirstOrDefaultAsync(gc => gc.Id == categoryId && gc.SubjectId == subjectId);

            if (category == null)
            {
                return new BaseResponse<GradeCategoryDto>
                {
                    Success = false,
                    Message = "Category not found",
                    StatusCode = 404
                };
            }

            // Check weights sum
            var otherWeightsSum = await _context.GradeCategories
                .Where(gc => gc.SubjectId == subjectId && gc.Id != categoryId)
                .SumAsync(gc => gc.Weight);

            if (otherWeightsSum + dto.Weight > 100)
            {
                return new BaseResponse<GradeCategoryDto>
                {
                    Success = false,
                    Message = "Grade category weights must total exactly 100%",
                    StatusCode = 400,
                    Errors = new List<string> { "Grade category weights must total exactly 100%." }
                };
            }

            category.Name = dto.Name;
            category.Weight = dto.Weight;
            await _context.SaveChangesAsync();

            return new BaseResponse<GradeCategoryDto>
            {
                Success = true,
                Message = "Category updated",
                StatusCode = 200,
                Data = new GradeCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Weight = category.Weight,
                    SubjectId = category.SubjectId
                }
            };
        }

        public async Task<BaseResponse<object>> DeleteCategoryAsync(Guid subjectId, Guid categoryId)
        {
            var category = await _context.GradeCategories
                .FirstOrDefaultAsync(gc => gc.Id == categoryId && gc.SubjectId == subjectId);

            if (category == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Category not found",
                    StatusCode = 404
                };
            }

            _context.GradeCategories.Remove(category);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Category deleted",
                StatusCode = 200
            };
        }
    }
}

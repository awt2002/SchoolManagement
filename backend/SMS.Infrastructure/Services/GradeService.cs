using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Grades.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class GradeService : IGradeService
    {
        private readonly AppDbContext _context;

        public GradeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<List<GradeDto>>> GetGradesAsync(
            Guid? subjectId, Guid? studentId, Guid? academicYearId)
        {
            var query = _context.Grades
                .Include(g => g.Student)
                .Include(g => g.GradeCategory)
                    .ThenInclude(gc => gc.Subject)
                .AsQueryable();

            if (studentId.HasValue)
            {
                query = query.Where(g => g.StudentId == studentId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(g => g.GradeCategory.SubjectId == subjectId.Value);
            }

            if (academicYearId.HasValue)
            {
                query = query.Where(g => g.GradeCategory.Subject.Class.AcademicYearId == academicYearId.Value);
            }

            var grades = await query
                .OrderBy(g => g.GradeCategory.Subject.Name)
                .ThenBy(g => g.GradeCategory.Name)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = g.Student.FullName,
                    GradeCategoryId = g.GradeCategoryId,
                    SubjectName = g.GradeCategory.Subject.Name,
                    CategoryName = g.GradeCategory.Name,
                    Score = g.Score,
                    WeightedContribution = g.Score * g.GradeCategory.Weight / 100m
                })
                .ToListAsync();

            return new BaseResponse<List<GradeDto>>
            {
                Success = true,
                Message = "Grades retrieved",
                StatusCode = 200,
                Data = grades
            };
        }

        public async Task<BaseResponse<GradeDto>> CreateOrUpdateGradeAsync(
            CreateGradeDto dto, Guid enteredByUserId)
        {
            // Check if grade already exists for this student and category (upsert)
            var existingGrade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == dto.StudentId
                    && g.GradeCategoryId == dto.GradeCategoryId);

            if (existingGrade != null)
            {
                // Log the change
                var auditLog = new GradeAuditLog
                {
                    Id = Guid.NewGuid(),
                    GradeId = existingGrade.Id,
                    OldScore = existingGrade.Score,
                    NewScore = dto.Score,
                    ChangedBy = enteredByUserId,
                    ChangedAt = DateTime.UtcNow
                };
                _context.GradeAuditLogs.Add(auditLog);

                try
                {
                    existingGrade.UpdateScore(dto.Score, enteredByUserId);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    return new BaseResponse<GradeDto>
                    {
                        Success = false,
                        Message = ex.Message,
                        StatusCode = 400,
                        Errors = new List<string> { ex.Message }
                    };
                }

                await _context.SaveChangesAsync();

                var updatedGradeDto = await BuildGradeDtoAsync(existingGrade.Id);
                return new BaseResponse<GradeDto>
                {
                    Success = true,
                    Message = "Grade updated",
                    StatusCode = 200,
                    Data = updatedGradeDto
                };
            }
            else
            {
                var student = await _context.Students
                    .Include(s => s.Grades)
                    .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

                if (student == null)
                {
                    return new BaseResponse<GradeDto>
                    {
                        Success = false,
                        Message = "Student not found",
                        StatusCode = 404
                    };
                }

                Grade grade;
                try
                {
                    grade = student.AssignGrade(dto.GradeCategoryId, dto.Score, enteredByUserId);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    return new BaseResponse<GradeDto>
                    {
                        Success = false,
                        Message = ex.Message,
                        StatusCode = 400,
                        Errors = new List<string> { ex.Message }
                    };
                }

                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                var createdGradeDto = await BuildGradeDtoAsync(grade.Id);
                return new BaseResponse<GradeDto>
                {
                    Success = true,
                    Message = "Grade created",
                    StatusCode = 201,
                    Data = createdGradeDto
                };
            }
        }

        private async Task<GradeDto> BuildGradeDtoAsync(Guid gradeId)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.GradeCategory)
                    .ThenInclude(gc => gc.Subject)
                .FirstAsync(g => g.Id == gradeId);

            return new GradeDto
            {
                Id = grade.Id,
                StudentId = grade.StudentId,
                StudentName = grade.Student.FullName,
                GradeCategoryId = grade.GradeCategoryId,
                SubjectName = grade.GradeCategory.Subject.Name,
                CategoryName = grade.GradeCategory.Name,
                Score = grade.Score,
                WeightedContribution = grade.Score * grade.GradeCategory.Weight / 100m
            };
        }

        public async Task<BaseResponse<GradeSummaryDto>> GetGradeSummaryAsync(
            Guid? studentId, Guid? classId, Guid? academicYearId)
        {
            var query = _context.Grades
                .Include(g => g.GradeCategory)
                    .ThenInclude(gc => gc.Subject)
                        .ThenInclude(s => s.Class)
                .AsQueryable();

            if (studentId.HasValue)
            {
                query = query.Where(g => g.StudentId == studentId.Value);
            }

            if (classId.HasValue)
            {
                query = query.Where(g => g.GradeCategory.Subject.ClassId == classId.Value);
            }

            if (academicYearId.HasValue)
            {
                query = query.Where(g => g.GradeCategory.Subject.Class.AcademicYearId == academicYearId.Value);
            }

            var grades = await query.ToListAsync();

            var subjectGroups = grades
                .GroupBy(g => new { g.GradeCategory.SubjectId, g.GradeCategory.Subject.Name })
                .Select(group =>
                {
                    var weightedSum = group.Sum(g => g.Score * g.GradeCategory.Weight / 100m);
                    return new SubjectAverageDto
                    {
                        SubjectId = group.Key.SubjectId,
                        SubjectName = group.Key.Name,
                        WeightedAverage = Math.Round(weightedSum, 2)
                    };
                })
                .ToList();

            var gpa = subjectGroups.Count > 0
                ? Math.Round(subjectGroups.Average(s => s.WeightedAverage), 2)
                : 0m;

            return new BaseResponse<GradeSummaryDto>
            {
                Success = true,
                Message = "Grade summary retrieved",
                StatusCode = 200,
                Data = new GradeSummaryDto
                {
                    SubjectAverages = subjectGroups,
                    Gpa = gpa
                }
            };
        }

        public async Task<PagedResponse<GradeAuditLogDto>> GetAuditLogAsync(
            Guid? studentId, Guid? subjectId, DateOnly? from, DateOnly? to, int page, int pageSize)
        {
            var query = _context.GradeAuditLogs
                .Include(a => a.Grade)
                    .ThenInclude(g => g.Student)
                .Include(a => a.Grade)
                    .ThenInclude(g => g.GradeCategory)
                        .ThenInclude(gc => gc.Subject)
                .Include(a => a.ChangedByUser)
                .AsQueryable();

            if (studentId.HasValue)
            {
                query = query.Where(a => a.Grade.StudentId == studentId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(a => a.Grade.GradeCategory.SubjectId == subjectId.Value);
            }

            if (from.HasValue)
            {
                var fromDate = from.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(a => a.ChangedAt >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(a => a.ChangedAt <= toDate);
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new GradeAuditLogDto
                {
                    Id = a.Id,
                    StudentName = a.Grade.Student.FullName,
                    SubjectName = a.Grade.GradeCategory.Subject.Name,
                    CategoryName = a.Grade.GradeCategory.Name,
                    OldScore = a.OldScore,
                    NewScore = a.NewScore,
                    ChangedBy = a.ChangedByUser.Username,
                    ChangedAt = a.ChangedAt
                })
                .ToListAsync();

            return new PagedResponse<GradeAuditLogDto>
            {
                Success = true,
                Message = "Audit log retrieved",
                StatusCode = 200,
                Data = logs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}

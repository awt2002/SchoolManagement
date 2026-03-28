using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class ExamService : IExamService
    {
        private readonly AppDbContext _context;

        public ExamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<List<ExamDto>>> GetExamsAsync(
            Guid? subjectId, Guid? classId, DateOnly? from, DateOnly? to)
        {
            var query = _context.Exams
                .Include(e => e.Subject)
                .AsQueryable();

            if (subjectId.HasValue)
            {
                query = query.Where(e => e.SubjectId == subjectId.Value);
            }

            if (classId.HasValue)
            {
                query = query.Where(e => e.Subject.ClassId == classId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.ExamDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.ExamDate <= to.Value);
            }

            var exams = await query
                .OrderByDescending(e => e.ExamDate)
                .Select(e => new ExamDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    SubjectId = e.SubjectId,
                    SubjectName = e.Subject.Name,
                    ExamDate = e.ExamDate,
                    MaxScore = e.MaxScore,
                    PassingThreshold = e.PassingThreshold
                })
                .ToListAsync();

            return new BaseResponse<List<ExamDto>>
            {
                Success = true,
                Message = "Exams retrieved",
                StatusCode = 200,
                Data = exams
            };
        }

        public async Task<BaseResponse<ExamDto>> CreateExamAsync(CreateExamDto dto, Guid createdByUserId)
        {
            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                SubjectId = dto.SubjectId,
                Name = dto.Name,
                ExamDate = dto.ExamDate,
                MaxScore = dto.MaxScore,
                PassingThreshold = dto.PassingThreshold,
                CreatedBy = createdByUserId
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var subject = await _context.Subjects.FindAsync(dto.SubjectId);

            return new BaseResponse<ExamDto>
            {
                Success = true,
                Message = "Exam created",
                StatusCode = 201,
                Data = new ExamDto
                {
                    Id = exam.Id,
                    Name = exam.Name,
                    SubjectId = exam.SubjectId,
                    SubjectName = subject?.Name ?? "",
                    ExamDate = exam.ExamDate,
                    MaxScore = exam.MaxScore,
                    PassingThreshold = exam.PassingThreshold
                }
            };
        }

        public async Task<BaseResponse<ExamDto>> GetExamByIdAsync(Guid id)
        {
            var exam = await _context.Exams
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return new BaseResponse<ExamDto>
                {
                    Success = false,
                    Message = "Exam not found",
                    StatusCode = 404
                };
            }

            return new BaseResponse<ExamDto>
            {
                Success = true,
                Message = "Exam retrieved",
                StatusCode = 200,
                Data = new ExamDto
                {
                    Id = exam.Id,
                    Name = exam.Name,
                    SubjectId = exam.SubjectId,
                    SubjectName = exam.Subject.Name,
                    ExamDate = exam.ExamDate,
                    MaxScore = exam.MaxScore,
                    PassingThreshold = exam.PassingThreshold
                }
            };
        }

        public async Task<BaseResponse<ExamDto>> UpdateExamAsync(Guid id, UpdateExamDto dto)
        {
            var exam = await _context.Exams
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return new BaseResponse<ExamDto>
                {
                    Success = false,
                    Message = "Exam not found",
                    StatusCode = 404
                };
            }

            exam.Name = dto.Name;
            exam.ExamDate = dto.ExamDate;
            exam.MaxScore = dto.MaxScore;
            exam.PassingThreshold = dto.PassingThreshold;

            await _context.SaveChangesAsync();

            return new BaseResponse<ExamDto>
            {
                Success = true,
                Message = "Exam updated",
                StatusCode = 200,
                Data = new ExamDto
                {
                    Id = exam.Id,
                    Name = exam.Name,
                    SubjectId = exam.SubjectId,
                    SubjectName = exam.Subject.Name,
                    ExamDate = exam.ExamDate,
                    MaxScore = exam.MaxScore,
                    PassingThreshold = exam.PassingThreshold
                }
            };
        }

        public async Task<BaseResponse<List<ExamResultDto>>> GetExamResultsAsync(Guid examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return new BaseResponse<List<ExamResultDto>>
                {
                    Success = false,
                    Message = "Exam not found",
                    StatusCode = 404
                };
            }

            var results = await _context.ExamResults
                .Include(er => er.Student)
                .Where(er => er.ExamId == examId)
                .Select(er => new ExamResultDto
                {
                    Id = er.Id,
                    StudentId = er.StudentId,
                    StudentName = er.Student.FullName,
                    Score = er.Score,
                    Percentage = exam.MaxScore > 0 ? Math.Round(er.Score / exam.MaxScore * 100, 2) : 0,
                    Passed = exam.MaxScore > 0 && (er.Score / exam.MaxScore * 100) >= exam.PassingThreshold
                })
                .ToListAsync();

            return new BaseResponse<List<ExamResultDto>>
            {
                Success = true,
                Message = "Exam results retrieved",
                StatusCode = 200,
                Data = results
            };
        }

        public async Task<BaseResponse<List<ExamResultDto>>> CreateExamResultsAsync(
            Guid examId, BulkExamResultDto dto, Guid enteredByUserId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return new BaseResponse<List<ExamResultDto>>
                {
                    Success = false,
                    Message = "Exam not found",
                    StatusCode = 404
                };
            }

            // Validate scores
            foreach (var entry in dto.Results)
            {
                if (entry.Score < 0 || entry.Score > exam.MaxScore)
                {
                    return new BaseResponse<List<ExamResultDto>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        StatusCode = 400,
                        Errors = new List<string> { $"Score must be between 0 and {exam.MaxScore}." }
                    };
                }
            }

            var resultEntities = new List<ExamResult>();

            foreach (var entry in dto.Results)
            {
                // Check if result already exists
                var existingResult = await _context.ExamResults
                    .FirstOrDefaultAsync(er => er.ExamId == examId && er.StudentId == entry.StudentId);

                if (existingResult != null)
                {
                    existingResult.Score = entry.Score;
                    existingResult.EnteredBy = enteredByUserId;
                    existingResult.EnteredAt = DateTime.UtcNow;
                    resultEntities.Add(existingResult);
                }
                else
                {
                    var result = new ExamResult
                    {
                        Id = Guid.NewGuid(),
                        ExamId = examId,
                        StudentId = entry.StudentId,
                        Score = entry.Score,
                        EnteredBy = enteredByUserId,
                        EnteredAt = DateTime.UtcNow
                    };
                    _context.ExamResults.Add(result);
                    resultEntities.Add(result);
                }
            }

            await _context.SaveChangesAsync();

            return await GetExamResultsAsync(examId);
        }

        public async Task<BaseResponse<ExamResultDetailDto>> GetStudentExamResultAsync(
            Guid examId, Guid studentId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return new BaseResponse<ExamResultDetailDto>
                {
                    Success = false,
                    Message = "Exam not found",
                    StatusCode = 404
                };
            }

            var result = await _context.ExamResults
                .FirstOrDefaultAsync(er => er.ExamId == examId && er.StudentId == studentId);

            if (result == null)
            {
                return new BaseResponse<ExamResultDetailDto>
                {
                    Success = false,
                    Message = "Exam result not found",
                    StatusCode = 404
                };
            }

            // Get all results for this student across exams in the same subject
            var allStudentResults = await _context.ExamResults
                .Include(er => er.Exam)
                .Where(er => er.StudentId == studentId && er.Exam.SubjectId == exam.SubjectId)
                .ToListAsync();

            var percentage = exam.MaxScore > 0 ? Math.Round(result.Score / exam.MaxScore * 100, 2) : 0;
            var passed = exam.MaxScore > 0 && percentage >= exam.PassingThreshold;

            var allScores = allStudentResults.Select(r => r.Score).ToList();
            var average = allScores.Count > 0 ? Math.Round(allScores.Average(), 2) : 0;
            var highest = allScores.Count > 0 ? allScores.Max() : 0;
            var lowest = allScores.Count > 0 ? allScores.Min() : 0;

            return new BaseResponse<ExamResultDetailDto>
            {
                Success = true,
                Message = "Exam result retrieved",
                StatusCode = 200,
                Data = new ExamResultDetailDto
                {
                    Score = result.Score,
                    Percentage = percentage,
                    Passed = passed,
                    Average = average,
                    Highest = highest,
                    Lowest = lowest
                }
            };
        }
    }
}

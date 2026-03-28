using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AttendanceService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<BaseResponse<List<AttendanceRecordDto>>> GetAttendanceAsync(
            Guid? classId, Guid? studentId, DateOnly? from, DateOnly? to)
        {
            var query = _context.AttendanceRecords
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.RecordedByUser)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(a => a.ClassId == classId.Value);
            }

            if (studentId.HasValue)
            {
                query = query.Where(a => a.StudentId == studentId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(a => a.AbsenceDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.AbsenceDate <= to.Value);
            }

            var records = await query
                .OrderByDescending(a => a.AbsenceDate)
                .Select(a => new AttendanceRecordDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    AbsenceDate = a.AbsenceDate,
                    RecordedByName = a.RecordedByUser.Username,
                    RecordedAt = a.RecordedAt
                })
                .ToListAsync();

            return new BaseResponse<List<AttendanceRecordDto>>
            {
                Success = true,
                Message = "Attendance records retrieved",
                StatusCode = 200,
                Data = records
            };
        }

        public async Task<BaseResponse<AttendanceRecordDto>> CreateAttendanceAsync(
            CreateAttendanceDto dto, Guid recordedByUserId)
        {
            var dateValidation = await ValidateAbsenceDateAsync(dto.AbsenceDate);
            if (dateValidation != null)
            {
                return dateValidation;
            }

            var duplicateValidation = await ValidateDuplicateRecordAsync(dto.StudentId, dto.AbsenceDate);
            if (duplicateValidation != null)
            {
                return duplicateValidation;
            }

            var student = await _context.Students
                .Include(s => s.ParentContact)
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

            if (student == null)
            {
                return new BaseResponse<AttendanceRecordDto>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            AttendanceRecord record;
            try
            {
                record = student.MarkAttendance(dto.ClassId, dto.AbsenceDate, recordedByUserId);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return new BaseResponse<AttendanceRecordDto>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = 400,
                    Errors = new List<string> { ex.Message }
                };
            }

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            // Send parent email
            try
            {
                if (student?.ParentContact != null)
                {
                    var totalAbsences = await _context.AttendanceRecords
                        .CountAsync(a => a.StudentId == dto.StudentId);

                    var subject = $"Absence Notice: {student.FullName}";
                    var body = $"Dear {student.ParentContact.FullName},\n\n" +
                               $"This is to inform you that {student.FullName} was absent from school on {dto.AbsenceDate:M/d/yyyy}.\n\n" +
                               $"Total absences this academic year: {totalAbsences}\n\n" +
                               $"If you have any questions, please contact the school administration.";

                    await _emailService.SendEmailAsync(student.ParentContact.Email, subject, body);
                }
            }
            catch
            {
                // Don't fail the request if email fails
            }

            var createdRecord = await _context.AttendanceRecords
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.RecordedByUser)
                .FirstAsync(a => a.Id == record.Id);

            return new BaseResponse<AttendanceRecordDto>
            {
                Success = true,
                Message = "Attendance recorded",
                StatusCode = 201,
                Data = new AttendanceRecordDto
                {
                    Id = createdRecord.Id,
                    StudentId = createdRecord.StudentId,
                    StudentName = createdRecord.Student.FullName,
                    ClassId = createdRecord.ClassId,
                    ClassName = createdRecord.Class.Name,
                    AbsenceDate = createdRecord.AbsenceDate,
                    RecordedByName = createdRecord.RecordedByUser.Username,
                    RecordedAt = createdRecord.RecordedAt
                }
            };
        }

        private async Task<BaseResponse<AttendanceRecordDto>?> ValidateAbsenceDateAsync(DateOnly absenceDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (absenceDate > today)
            {
                return new BaseResponse<AttendanceRecordDto>
                {
                    Success = false,
                    Message = "Absence date cannot be in the future",
                    StatusCode = 400,
                    Errors = new List<string> { "Absence date cannot be in the future" }
                };
            }

            var activeYear = await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.IsActive);

            if (activeYear != null && (absenceDate < activeYear.StartDate || absenceDate > activeYear.EndDate))
            {
                return new BaseResponse<AttendanceRecordDto>
                {
                    Success = false,
                    Message = "Absence date must be within the current academic year",
                    StatusCode = 400,
                    Errors = new List<string> { "Absence date must be within the current academic year" }
                };
            }

            return null;
        }

        private async Task<BaseResponse<AttendanceRecordDto>?> ValidateDuplicateRecordAsync(Guid studentId, DateOnly absenceDate)
        {
            var exists = await _context.AttendanceRecords
                .AnyAsync(a => a.StudentId == studentId && a.AbsenceDate == absenceDate);

            if (!exists)
            {
                return null;
            }

            return new BaseResponse<AttendanceRecordDto>
            {
                Success = false,
                Message = "Attendance record already exists for this student on this date",
                StatusCode = 409,
                Errors = new List<string> { "Duplicate attendance record" }
            };
        }

        public async Task<BaseResponse<object>> DeleteAttendanceAsync(Guid id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Attendance record not found",
                    StatusCode = 404
                };
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Attendance record deleted",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<AttendanceSummaryDto>> GetStudentSummaryAsync(
            Guid studentId, Guid? academicYearId)
        {
            var query = _context.AttendanceRecords
                .Where(a => a.StudentId == studentId);

            if (academicYearId.HasValue)
            {
                var year = await _context.AcademicYears.FindAsync(academicYearId.Value);
                if (year != null)
                {
                    query = query.Where(a => a.AbsenceDate >= year.StartDate && a.AbsenceDate <= year.EndDate);
                }
            }
            else
            {
                var activeYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsActive);
                if (activeYear != null)
                {
                    query = query.Where(a => a.AbsenceDate >= activeYear.StartDate && a.AbsenceDate <= activeYear.EndDate);
                }
            }

            var absenceDates = await query
                .OrderByDescending(a => a.AbsenceDate)
                .Select(a => a.AbsenceDate)
                .ToListAsync();

            return new BaseResponse<AttendanceSummaryDto>
            {
                Success = true,
                Message = "Attendance summary retrieved",
                StatusCode = 200,
                Data = new AttendanceSummaryDto
                {
                    TotalAbsences = absenceDates.Count,
                    AbsenceDates = absenceDates
                }
            };
        }
    }
}

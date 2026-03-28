using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Classes.DTOs;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class ClassService : IClassService
    {
        private readonly AppDbContext _context;

        public ClassService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<List<ClassDto>>> GetAllClassesAsync(Guid? academicYearId, Guid? teacherId)
        {
            var query = _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.AcademicYear)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (academicYearId.HasValue)
            {
                query = query.Where(c => c.AcademicYearId == academicYearId.Value);
            }

            if (teacherId.HasValue)
            {
                query = query.Where(c => c.TeacherId == teacherId.Value);
            }

            var classes = await query
                .OrderBy(c => c.GradeLevel)
                .ThenBy(c => c.Name)
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    GradeLevel = c.GradeLevel,
                    TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                    TeacherId = c.TeacherId,
                    StudentCount = c.Enrollments.Count(e => e.Student.User.Role == UserRole.Student),
                    AcademicYearId = c.AcademicYearId,
                    AcademicYearName = c.AcademicYear.Name
                })
                .ToListAsync();

            return new BaseResponse<List<ClassDto>>
            {
                Success = true,
                Message = "Classes retrieved",
                StatusCode = 200,
                Data = classes
            };
        }

        public async Task<BaseResponse<ClassDto>> CreateClassAsync(CreateClassDto dto)
        {
            if (dto.TeacherId.HasValue)
            {
                var teacherAlreadyAssigned = await _context.Teachers
                    .AnyAsync(t => t.Id == dto.TeacherId.Value && t.ClassId != null);
                if (teacherAlreadyAssigned)
                {
                    return new BaseResponse<ClassDto>
                    {
                        Success = false,
                        Message = "This teacher is already assigned to another class. Please unassign them first.",
                        StatusCode = 409,
                        Errors = new List<string> { "This teacher is already assigned to another class. Please unassign them first." }
                    };
                }
            }

            var newClass = new Class
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                GradeLevel = dto.GradeLevel,
                TeacherId = dto.TeacherId,
                AcademicYearId = dto.AcademicYearId
            };

            _context.Classes.Add(newClass);

            if (dto.TeacherId.HasValue)
            {
                var teacher = await _context.Teachers.FindAsync(dto.TeacherId.Value);
                if (teacher != null)
                {
                    teacher.ClassId = newClass.Id;
                }
            }

            await _context.SaveChangesAsync();

            var academicYear = await _context.AcademicYears.FindAsync(dto.AcademicYearId);

            return new BaseResponse<ClassDto>
            {
                Success = true,
                Message = "Class created",
                StatusCode = 201,
                Data = new ClassDto
                {
                    Id = newClass.Id,
                    Name = newClass.Name,
                    GradeLevel = newClass.GradeLevel,
                    TeacherId = newClass.TeacherId,
                    StudentCount = 0,
                    AcademicYearId = newClass.AcademicYearId,
                    AcademicYearName = academicYear?.Name ?? ""
                }
            };
        }

        public async Task<BaseResponse<ClassDetailDto>> GetClassByIdAsync(Guid id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.AcademicYear)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.User)
                .Include(c => c.Subjects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                return new BaseResponse<ClassDetailDto>
                {
                    Success = false,
                    Message = "Class not found",
                    StatusCode = 404
                };
            }

            var dto = new ClassDetailDto
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                GradeLevel = classEntity.GradeLevel,
                TeacherName = classEntity.Teacher?.FullName,
                TeacherId = classEntity.TeacherId,
                StudentCount = classEntity.Enrollments.Count(e => e.Student.User.Role == UserRole.Student),
                AcademicYearId = classEntity.AcademicYearId,
                AcademicYearName = classEntity.AcademicYear.Name,
                Students = classEntity.Enrollments
                    .Where(e => e.Student.User.Role == UserRole.Student)
                    .Select(e => new ClassStudentDto
                    {
                        Id = e.Student.Id,
                        FullName = e.Student.FullName
                    })
                    .ToList(),
                Subjects = classEntity.Subjects.Select(s => new ClassSubjectDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList()
            };

            return new BaseResponse<ClassDetailDto>
            {
                Success = true,
                Message = "Class retrieved",
                StatusCode = 200,
                Data = dto
            };
        }

        public async Task<BaseResponse<ClassDto>> UpdateClassAsync(Guid id, UpdateClassDto dto)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.AcademicYear)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                return new BaseResponse<ClassDto>
                {
                    Success = false,
                    Message = "Class not found",
                    StatusCode = 404
                };
            }

            if (dto.TeacherId.HasValue && dto.TeacherId != classEntity.TeacherId)
            {
                var teacherAlreadyAssigned = await _context.Teachers
                    .AnyAsync(t => t.Id == dto.TeacherId.Value && t.ClassId != null && t.ClassId != id);
                if (teacherAlreadyAssigned)
                {
                    return new BaseResponse<ClassDto>
                    {
                        Success = false,
                        Message = "This teacher is already assigned to another class. Please unassign them first.",
                        StatusCode = 409,
                        Errors = new List<string> { "This teacher is already assigned to another class. Please unassign them first." }
                    };
                }
            }

            // Unassign old teacher
            if (classEntity.TeacherId.HasValue && classEntity.TeacherId != dto.TeacherId)
            {
                var oldTeacher = await _context.Teachers.FindAsync(classEntity.TeacherId.Value);
                if (oldTeacher != null)
                {
                    oldTeacher.ClassId = null;
                }
            }

            classEntity.Name = dto.Name;
            classEntity.GradeLevel = dto.GradeLevel;
            classEntity.TeacherId = dto.TeacherId;
            classEntity.AcademicYearId = dto.AcademicYearId;

            foreach (var enrollment in classEntity.Enrollments)
            {
                enrollment.AcademicYearId = dto.AcademicYearId;
            }

            if (dto.TeacherId.HasValue)
            {
                var newTeacher = await _context.Teachers.FindAsync(dto.TeacherId.Value);
                if (newTeacher != null)
                {
                    newTeacher.ClassId = classEntity.Id;
                }
            }

            await _context.SaveChangesAsync();

            var studentCount = await _context.Enrollments.CountAsync(e => e.ClassId == id);
            var academicYearName = await _context.AcademicYears
                .Where(a => a.Id == classEntity.AcademicYearId)
                .Select(a => a.Name)
                .FirstOrDefaultAsync();

            return new BaseResponse<ClassDto>
            {
                Success = true,
                Message = "Class updated",
                StatusCode = 200,
                Data = new ClassDto
                {
                    Id = classEntity.Id,
                    Name = classEntity.Name,
                    GradeLevel = classEntity.GradeLevel,
                    TeacherId = classEntity.TeacherId,
                    StudentCount = studentCount,
                    AcademicYearId = classEntity.AcademicYearId,
                    AcademicYearName = academicYearName ?? string.Empty
                }
            };
        }

        public async Task<BaseResponse<object>> DeleteClassAsync(Guid id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Enrollments)
                .Include(c => c.AttendanceRecords)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Class not found",
                    StatusCode = 404
                };
            }

            if (classEntity.Enrollments.Count > 0)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Cannot delete class with enrolled students",
                    StatusCode = 409,
                    Errors = new List<string> { "Remove enrolled students before deleting this class." }
                };
            }

            var assignedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.ClassId == id);
            if (assignedTeacher != null)
            {
                assignedTeacher.ClassId = null;
            }

            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Class deleted",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<EnrollmentDto>> EnrollStudentAsync(Guid classId, EnrollStudentDto dto)
        {
            var classEntity = await _context.Classes
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
            {
                return new BaseResponse<EnrollmentDto>
                {
                    Success = false,
                    Message = "Class not found",
                    StatusCode = 404
                };
            }

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == dto.StudentId && e.ClassId == classId);
            if (alreadyEnrolled)
            {
                return new BaseResponse<EnrollmentDto>
                {
                    Success = false,
                    Message = "Student is already enrolled in this class",
                    StatusCode = 409,
                    Errors = new List<string> { "Student is already enrolled in this class" }
                };
            }

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = dto.StudentId,
                ClassId = classId,
                AcademicYearId = classEntity.AcademicYearId,
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return new BaseResponse<EnrollmentDto>
            {
                Success = true,
                Message = "Student enrolled",
                StatusCode = 201,
                Data = new EnrollmentDto
                {
                    Id = enrollment.Id,
                    ClassName = classEntity.Name,
                    AcademicYearName = classEntity.AcademicYear.Name,
                    EnrolledAt = enrollment.EnrolledAt
                }
            };
        }

        public async Task<BaseResponse<object>> RemoveStudentFromClassAsync(Guid classId, Guid studentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == studentId);

            if (enrollment == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Enrollment not found",
                    StatusCode = 404
                };
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Student removed from class",
                StatusCode = 200
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public StudentService(AppDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<PagedResponse<StudentSummaryDto>> GetAllStudentsAsync(
            int page, int pageSize, string? search, int? gradeLevel,
            Guid? classId, Guid? academicYearId, bool includeInactive, bool inactiveOnly = false)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .AsQueryable();

            query = query.Where(s => s.User.Role == UserRole.Student);

            if (inactiveOnly)
            {
                query = query.Where(s => !s.User.IsActive);
            }
            else if (!includeInactive)
            {
                query = query.Where(s => s.User.IsActive);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.FullName.Contains(search));
            }

            if (gradeLevel.HasValue)
            {
                query = query.Where(s => s.Enrollments.Any(e => e.Class.GradeLevel == gradeLevel.Value));
            }

            if (classId.HasValue)
            {
                query = query.Where(s => s.Enrollments.Any(e => e.ClassId == classId.Value));
            }

            if (academicYearId.HasValue)
            {
                query = query.Where(s => s.Enrollments.Any(e => e.AcademicYearId == academicYearId.Value));
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    DateOfBirth = s.DateOfBirth,
                    PhotoUrl = s.PhotoUrl,
                    ClassName = s.Enrollments
                        .OrderByDescending(e => e.EnrolledAt)
                        .Select(e => e.Class.Name)
                        .FirstOrDefault() ?? "",
                    GradeLevel = s.Enrollments
                        .OrderByDescending(e => e.EnrolledAt)
                        .Select(e => e.Class.GradeLevel)
                        .FirstOrDefault(),
                    IsActive = s.User.IsActive
                })
                .ToListAsync();

            return new PagedResponse<StudentSummaryDto>
            {
                Success = true,
                Message = "Students retrieved",
                StatusCode = 200,
                Data = students,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<BaseResponse<StudentDetailDto>> GetStudentByIdAsync(Guid id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ParentContact)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.AcademicYear)
                .FirstOrDefaultAsync(s => s.Id == id && s.User.Role == UserRole.Student);

            if (student == null)
            {
                return new BaseResponse<StudentDetailDto>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            var dto = new StudentDetailDto
            {
                Id = student.Id,
                UserId = student.UserId,
                Username = student.User.Username,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                PhotoUrl = student.PhotoUrl,
                EnrollmentYear = student.EnrollmentYear,
                IsActive = student.User.IsActive,
                CurrentClassId = student.Enrollments
                    .OrderByDescending(e => e.EnrolledAt)
                    .Select(e => (Guid?)e.ClassId)
                    .FirstOrDefault(),
                ParentName = student.ParentContact?.FullName ?? "",
                ParentEmail = student.ParentContact?.Email ?? "",
                ParentPhone = student.ParentContact?.PhoneNumber ?? "",
                Enrollments = student.Enrollments.Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    ClassName = e.Class.Name,
                    AcademicYearName = e.AcademicYear.Name,
                    EnrolledAt = e.EnrolledAt
                }).ToList()
            };

            return new BaseResponse<StudentDetailDto>
            {
                Success = true,
                Message = "Student retrieved",
                StatusCode = 200,
                Data = dto
            };
        }

        public async Task<BaseResponse<StudentDetailDto>> GetStudentByUserIdAsync(Guid userId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.User.Role == UserRole.Student);

            if (student == null)
            {
                return new BaseResponse<StudentDetailDto>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            return await GetStudentByIdAsync(student.Id);
        }

        public async Task<BaseResponse<StudentDetailDto>> CreateStudentAsync(CreateStudentDto dto)
        {
            var username = await GenerateUniqueStudentUsernameAsync(dto.FullName);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = AuthService.HashPassword("Student123"),
                Email = $"{username}@school.com",
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                EnrollmentYear = DateTime.UtcNow.Year
            };

            _context.Students.Add(student);

            var parentContact = new ParentContact
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                FullName = dto.ParentName,
                Email = dto.ParentEmail,
                PhoneNumber = dto.ParentPhone
            };

            _context.ParentContacts.Add(parentContact);

            // Enroll in class if requested
            if (dto.ClassId.HasValue || dto.AcademicYearId.HasValue)
            {
                var enrollmentInputValidation = ValidateEnrollmentInput(dto.ClassId, dto.AcademicYearId);
                if (enrollmentInputValidation != null)
                {
                    return enrollmentInputValidation;
                }

                var classEntity = await _context.Classes.FindAsync(dto.ClassId.Value);
                if (classEntity != null)
                {
                    try
                    {
                        var enrollment = student.EnrollStudent(dto.ClassId.Value, dto.AcademicYearId.Value);
                        _context.Enrollments.Add(enrollment);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return new BaseResponse<StudentDetailDto>
                        {
                            Success = false,
                            Message = ex.Message,
                            StatusCode = 400,
                            Errors = new List<string> { ex.Message }
                        };
                    }
                }
            }

            await _context.SaveChangesAsync();

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://localhost:7056";
            var loginLink = $"{frontendBaseUrl.TrimEnd('/')}/login";
            var recipientEmail = string.IsNullOrWhiteSpace(dto.ParentEmail) ? user.Email : dto.ParentEmail;

            await _emailService.SendEmailAsync(
                recipientEmail,
                "Welcome to SMS - Student Account",
                $"Welcome to the School Management System.\n\n" +
                $"Student Name: {dto.FullName}\n" +
                $"Username: {username}\n" +
                "Password: Student123\n\n" +
                $"Login here: {loginLink}\n\n" +
                "Please change the password after first login.");

            return await GetStudentByIdAsync(student.Id);
        }

        private async Task<string> GenerateUniqueStudentUsernameAsync(string fullName)
        {
            var username = fullName.Replace(" ", "").ToLower();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var counter = 1;
            var originalUsername = username;

            while (existingUser != null)
            {
                username = originalUsername + counter;
                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                counter++;
            }

            if (username.Length > 20)
            {
                username = username.Substring(0, 16) + new Random().Next(1000, 9999).ToString();
            }

            return username;
        }

        private static BaseResponse<StudentDetailDto>? ValidateEnrollmentInput(Guid? classId, Guid? academicYearId)
        {
            if (classId.HasValue && academicYearId.HasValue)
            {
                return null;
            }

            return new BaseResponse<StudentDetailDto>
            {
                Success = false,
                Message = "Class and academic year must both be provided when enrolling a student",
                StatusCode = 400,
                Errors = new List<string> { "Class and academic year must both be provided when enrolling a student" }
            };
        }

        public async Task<BaseResponse<StudentDetailDto>> UpdateStudentAsync(Guid id, UpdateStudentDto dto)
        {
            var student = await _context.Students
                .Include(s => s.ParentContact)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return new BaseResponse<StudentDetailDto>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            student.FullName = dto.FullName;
            student.DateOfBirth = dto.DateOfBirth;
            student.Address = dto.Address;

            if (student.ParentContact != null)
            {
                student.ParentContact.FullName = dto.ParentName;
                student.ParentContact.Email = dto.ParentEmail;
                student.ParentContact.PhoneNumber = dto.ParentPhone;
            }

            await _context.SaveChangesAsync();

            return await GetStudentByIdAsync(id);
        }

        public async Task<BaseResponse<object>> DeleteStudentAsync(Guid id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            student.User.IsActive = false;

            // Revoke all refresh tokens
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == student.UserId && rt.RevokedAt == null)
                .ToListAsync();
            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Student deactivated",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<object>> ReactivateStudentAsync(Guid id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            student.User.IsActive = true;
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Student reactivated",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<object>> UploadPhotoAsync(Guid id, string fileName, Stream fileStream)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Student not found",
                    StatusCode = 404
                };
            }

            var extension = Path.GetExtension(fileName).ToLower();
            var newFileName = $"{id}{extension}";
            var uploadPath = Path.Combine("wwwroot", "uploads", "students");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, newFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            student.PhotoUrl = $"/uploads/students/{newFileName}";
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Photo uploaded",
                StatusCode = 200,
                Data = new { photoUrl = student.PhotoUrl }
            };
        }

        public async Task<BaseResponse<List<EnrollmentDto>>> GetEnrollmentsAsync(Guid studentId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Class)
                .Include(e => e.AcademicYear)
                .Where(e => e.StudentId == studentId)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    ClassName = e.Class.Name,
                    AcademicYearName = e.AcademicYear.Name,
                    EnrolledAt = e.EnrolledAt
                })
                .ToListAsync();

            return new BaseResponse<List<EnrollmentDto>>
            {
                Success = true,
                Message = "Enrollments retrieved",
                StatusCode = 200,
                Data = enrollments
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using SMS.Application.Common;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public TeacherService(AppDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<PagedResponse<TeacherDto>> GetAllTeachersAsync(int page, int pageSize, string? search, bool includeInactive, bool inactiveOnly = false)
        {
            var query = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Class)
                .AsQueryable();

            if (inactiveOnly)
            {
                query = query.Where(t => !t.User.IsActive);
            }
            else if (!includeInactive)
            {
                query = query.Where(t => t.User.IsActive);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.FullName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    FullName = t.FullName,
                    Email = t.User.Email,
                    PhoneNumber = t.PhoneNumber ?? string.Empty,
                    Username = t.User.Username,
                    IsActive = t.User.IsActive,
                    AssignedClassName = t.Class != null ? t.Class.Name : null,
                    ClassId = t.ClassId
                })
                .ToListAsync();

            return new PagedResponse<TeacherDto>
            {
                Success = true,
                Message = "Teachers retrieved",
                StatusCode = 200,
                Data = teachers,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<BaseResponse<TeacherDto>> GetTeacherByIdAsync(Guid id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Class)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Teacher not found",
                    StatusCode = 404
                };
            }

            return new BaseResponse<TeacherDto>
            {
                Success = true,
                Message = "Teacher retrieved",
                StatusCode = 200,
                Data = MapTeacherDto(teacher)
            };
        }

        public async Task<BaseResponse<TeacherDto>> GetTeacherByUserIdAsync(Guid userId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Class)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Teacher not found",
                    StatusCode = 404
                };
            }

            return new BaseResponse<TeacherDto>
            {
                Success = true,
                Message = "Teacher retrieved",
                StatusCode = 200,
                Data = MapTeacherDto(teacher)
            };
        }

        public async Task<BaseResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto)
        {
            var createValidation = await ValidateCreateTeacherAsync(dto);
            if (createValidation != null)
            {
                return createValidation;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = AuthService.HashPassword("Teacher123"),
                Email = dto.Email,
                Role = UserRole.Teacher,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            var teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                ClassId = dto.ClassId
            };

            _context.Teachers.Add(teacher);

            if (dto.ClassId.HasValue)
            {
                var classToUpdate = await _context.Classes.FindAsync(dto.ClassId.Value);
                if (classToUpdate != null)
                {
                    classToUpdate.TeacherId = teacher.Id;
                }
            }

            await _context.SaveChangesAsync();

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://localhost:7056";
            var loginLink = $"{frontendBaseUrl.TrimEnd('/')}/login";
            await _emailService.SendEmailAsync(
                dto.Email,
                "Welcome to SMS - Teacher Account",
                $"Welcome to the School Management System.\n\n" +
                $"Username: {dto.Username}\n" +
                "Password: Teacher123\n\n" +
                $"Login here: {loginLink}\n\n" +
                "Please change your password after first login.");

            return await GetTeacherByIdAsync(teacher.Id);
        }

        public async Task<BaseResponse<TeacherDto>> UpdateTeacherAsync(Guid id, UpdateTeacherDto dto)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Class)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Teacher not found",
                    StatusCode = 404
                };
            }

            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != teacher.UserId);
            if (existingEmail != null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Email already exists",
                    StatusCode = 409,
                    Errors = new List<string> { "Email already exists" }
                };
            }

            if (dto.ClassId.HasValue && dto.ClassId != teacher.ClassId)
            {
                var existingTeacherForClass = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.ClassId == dto.ClassId.Value && t.Id != id);
                if (existingTeacherForClass != null)
                {
                    return new BaseResponse<TeacherDto>
                    {
                        Success = false,
                        Message = "This teacher is already assigned to another class. Please unassign them first.",
                        StatusCode = 409,
                        Errors = new List<string> { "This teacher is already assigned to another class. Please unassign them first." }
                    };
                }
            }

            if (teacher.ClassId.HasValue && teacher.ClassId != dto.ClassId)
            {
                var oldClass = await _context.Classes.FindAsync(teacher.ClassId.Value);
                if (oldClass != null)
                {
                    oldClass.TeacherId = null;
                }
            }

            teacher.FullName = dto.FullName;
            teacher.User.Email = dto.Email;
            teacher.PhoneNumber = dto.PhoneNumber;
            teacher.ClassId = dto.ClassId;

            if (dto.ClassId.HasValue)
            {
                var newClass = await _context.Classes.FindAsync(dto.ClassId.Value);
                if (newClass != null)
                {
                    newClass.TeacherId = teacher.Id;
                }
            }

            await _context.SaveChangesAsync();

            return await GetTeacherByIdAsync(id);
        }

        public async Task<BaseResponse<object>> DeleteTeacherAsync(Guid id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Teacher not found",
                    StatusCode = 404
                };
            }

            teacher.User.IsActive = false;

            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == teacher.UserId && rt.RevokedAt == null)
                .ToListAsync();
            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Teacher deactivated",
                StatusCode = 200
            };
        }

        public async Task<BaseResponse<object>> ReactivateTeacherAsync(Guid id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = "Teacher not found",
                    StatusCode = 404
                };
            }

            teacher.User.IsActive = true;
            await _context.SaveChangesAsync();

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Teacher reactivated",
                StatusCode = 200
            };
        }

        private static TeacherDto MapTeacherDto(Teacher teacher)
        {
            return new TeacherDto
            {
                Id = teacher.Id,
                UserId = teacher.UserId,
                FullName = teacher.FullName,
                Email = teacher.User.Email,
                PhoneNumber = teacher.PhoneNumber ?? string.Empty,
                Username = teacher.User.Username,
                IsActive = teacher.User.IsActive,
                AssignedClassName = teacher.Class?.Name,
                ClassId = teacher.ClassId
            };
        }

        private async Task<BaseResponse<TeacherDto>?> ValidateCreateTeacherAsync(CreateTeacherDto dto)
        {
            if (!MailAddress.TryCreate(dto.Email, out _))
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Please enter a valid email address.",
                    StatusCode = 400,
                    Errors = new List<string> { "Please enter a valid email address." }
                };
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (existingUser != null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Username already exists",
                    StatusCode = 409,
                    Errors = new List<string> { "Username already exists" }
                };
            }

            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingEmail != null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "Email already exists",
                    StatusCode = 409,
                    Errors = new List<string> { "Email already exists" }
                };
            }

            if (!dto.ClassId.HasValue)
            {
                return null;
            }

            var existingTeacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.ClassId == dto.ClassId.Value);
            if (existingTeacher != null)
            {
                return new BaseResponse<TeacherDto>
                {
                    Success = false,
                    Message = "This teacher is already assigned to another class. Please unassign them first.",
                    StatusCode = 409,
                    Errors = new List<string> { "This teacher is already assigned to another class. Please unassign them first." }
                };
            }

            return null;
        }
    }
}

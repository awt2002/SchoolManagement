using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Announcements.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AnnouncementService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<PagedResponse<AnnouncementDto>> GetAnnouncementsAsync(
            Guid userId, string userRole, Guid? classId, int page, int pageSize)
        {
            var query = _context.Announcements
                .Include(a => a.Author)
                .Include(a => a.Class)
                .Include(a => a.ReadStatuses)
                .AsQueryable();

            // Filter by visibility
            if (userRole == "Student" || userRole == "Teacher")
            {
                if (classId.HasValue)
                {
                    query = query.Where(a =>
                        a.Scope == AnnouncementScope.SchoolWide ||
                        (a.Scope == AnnouncementScope.ClassOnly && a.ClassId == classId.Value));
                }
                else
                {
                    query = query.Where(a => a.Scope == AnnouncementScope.SchoolWide);
                }
            }

            var totalCount = await query.CountAsync();

            var announcements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Body = a.Body,
                    Scope = a.Scope.ToString(),
                    AuthorName = a.Author.Username,
                    ClassId = a.ClassId,
                    ClassName = a.Class != null ? a.Class.Name : null,
                    CreatedAt = a.CreatedAt,
                    IsRead = a.ReadStatuses.Any(rs => rs.UserId == userId)
                })
                .ToListAsync();

            return new PagedResponse<AnnouncementDto>
            {
                Success = true,
                Message = "Announcements retrieved",
                StatusCode = 200,
                Data = announcements,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<BaseResponse<AnnouncementDto>> CreateAnnouncementAsync(
            CreateAnnouncementDto dto, Guid authorId)
        {
            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Body = dto.Body,
                Scope = dto.Scope,
                ClassId = dto.ClassId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // Send emails
            try
            {
                var author = await _context.Users.FindAsync(authorId);
                var authorName = author?.Username ?? "System";

                List<User> targetUsers;

                if (dto.Scope == AnnouncementScope.SchoolWide)
                {
                    targetUsers = await _context.Users
                        .Where(u => u.IsActive)
                        .ToListAsync();
                }
                else
                {
                    // Get students in the class
                    var studentUserIds = await _context.Enrollments
                        .Where(e => e.ClassId == dto.ClassId)
                        .Include(e => e.Student)
                        .Select(e => e.Student.UserId)
                        .ToListAsync();

                    // Get the teacher of the class
                    var teacherUserId = await _context.Teachers
                        .Where(t => t.ClassId == dto.ClassId)
                        .Select(t => t.UserId)
                        .FirstOrDefaultAsync();

                    var userIds = new List<Guid>(studentUserIds);
                    if (teacherUserId != Guid.Empty)
                    {
                        userIds.Add(teacherUserId);
                    }

                    targetUsers = await _context.Users
                        .Where(u => userIds.Contains(u.Id) && u.IsActive)
                        .ToListAsync();
                }

                foreach (var user in targetUsers)
                {
                    var subject = $"New Announcement: {announcement.Title}";
                    var body = $"Dear {user.Username},\n\n" +
                               $"A new announcement has been posted by {authorName}:\n\n" +
                               $"Title: {announcement.Title}\n\n" +
                               $"{announcement.Body}\n\n" +
                               $"Please log in to the School Management System to view the full announcement.";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }
            catch
            {
                // Don't fail if email sending fails
            }

            var createdAnnouncement = await _context.Announcements
                .Include(a => a.Author)
                .Include(a => a.Class)
                .FirstAsync(a => a.Id == announcement.Id);

            return new BaseResponse<AnnouncementDto>
            {
                Success = true,
                Message = "Announcement created",
                StatusCode = 201,
                Data = new AnnouncementDto
                {
                    Id = createdAnnouncement.Id,
                    Title = createdAnnouncement.Title,
                    Body = createdAnnouncement.Body,
                    Scope = createdAnnouncement.Scope.ToString(),
                    AuthorName = createdAnnouncement.Author.Username,
                    ClassId = createdAnnouncement.ClassId,
                    ClassName = createdAnnouncement.Class?.Name,
                    CreatedAt = createdAnnouncement.CreatedAt,
                    IsRead = false
                }
            };
        }

        public async Task<BaseResponse<AnnouncementDto>> GetAnnouncementByIdAsync(Guid id, Guid userId)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Author)
                .Include(a => a.Class)
                .Include(a => a.ReadStatuses)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return new BaseResponse<AnnouncementDto>
                {
                    Success = false,
                    Message = "Announcement not found",
                    StatusCode = 404
                };
            }

            return new BaseResponse<AnnouncementDto>
            {
                Success = true,
                Message = "Announcement retrieved",
                StatusCode = 200,
                Data = new AnnouncementDto
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Body = announcement.Body,
                    Scope = announcement.Scope.ToString(),
                    AuthorName = announcement.Author.Username,
                    ClassId = announcement.ClassId,
                    ClassName = announcement.Class?.Name,
                    CreatedAt = announcement.CreatedAt,
                    IsRead = announcement.ReadStatuses.Any(rs => rs.UserId == userId)
                }
            };
        }

        public async Task<BaseResponse<object>> MarkAsReadAsync(Guid announcementId, Guid userId)
        {
            var exists = await _context.AnnouncementReadStatuses
                .AnyAsync(rs => rs.AnnouncementId == announcementId && rs.UserId == userId);

            if (!exists)
            {
                var readStatus = new AnnouncementReadStatus
                {
                    Id = Guid.NewGuid(),
                    AnnouncementId = announcementId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                };

                _context.AnnouncementReadStatuses.Add(readStatus);
                await _context.SaveChangesAsync();
            }

            return new BaseResponse<object>
            {
                Success = true,
                Message = "Marked as read",
                StatusCode = 200,
                Data = new { readAt = DateTime.UtcNow }
            };
        }

        public async Task<BaseResponse<UnreadCountDto>> GetUnreadCountAsync(
            Guid userId, string userRole, Guid? classId)
        {
            var query = _context.Announcements.AsQueryable();

            if (userRole == "Student" || userRole == "Teacher")
            {
                if (classId.HasValue)
                {
                    query = query.Where(a =>
                        a.Scope == AnnouncementScope.SchoolWide ||
                        (a.Scope == AnnouncementScope.ClassOnly && a.ClassId == classId.Value));
                }
                else
                {
                    query = query.Where(a => a.Scope == AnnouncementScope.SchoolWide);
                }
            }

            var readAnnouncementIds = await _context.AnnouncementReadStatuses
                .Where(rs => rs.UserId == userId)
                .Select(rs => rs.AnnouncementId)
                .ToListAsync();

            var unreadCount = await query
                .CountAsync(a => !readAnnouncementIds.Contains(a.Id));

            return new BaseResponse<UnreadCountDto>
            {
                Success = true,
                Message = "Unread count retrieved",
                StatusCode = 200,
                Data = new UnreadCountDto { Count = unreadCount }
            };
        }
    }
}

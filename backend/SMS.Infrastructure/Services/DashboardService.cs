using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Features.Dashboard.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<AdminDashboardDto>> GetAdminDashboardAsync(Guid userId)
        {
            var totalStudents = await _context.Students
                .Include(s => s.User)
                .CountAsync(s => s.User.IsActive);

            var totalTeachers = await _context.Teachers
                .Include(t => t.User)
                .CountAsync(t => t.User.IsActive);

            var totalClasses = await _context.Classes.CountAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var absencesToday = await _context.AttendanceRecords
                .CountAsync(a => a.AbsenceDate == today);

            var readIds = await _context.AnnouncementReadStatuses
                .Where(rs => rs.UserId == userId)
                .Select(rs => rs.AnnouncementId)
                .ToListAsync();

            var unreadAnnouncements = await _context.Announcements
                .CountAsync(a => !readIds.Contains(a.Id));

            var recentAnnouncements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new DashboardAnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    CreatedAt = a.CreatedAt,
                    IsRead = readIds.Contains(a.Id)
                })
                .ToListAsync();

            return new BaseResponse<AdminDashboardDto>
            {
                Success = true,
                Message = "Dashboard retrieved",
                StatusCode = 200,
                Data = new AdminDashboardDto
                {
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalClasses = totalClasses,
                    AbsencesToday = absencesToday,
                    UnreadAnnouncements = unreadAnnouncements,
                    RecentAnnouncements = recentAnnouncements
                }
            };
        }

        public async Task<BaseResponse<TeacherDashboardDto>> GetTeacherDashboardAsync(Guid userId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Class)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher?.ClassId == null)
            {
                return new BaseResponse<TeacherDashboardDto>
                {
                    Success = true,
                    Message = "Dashboard retrieved",
                    StatusCode = 200,
                    Data = new TeacherDashboardDto()
                };
            }

            var classId = teacher.ClassId.Value;

            var classStudentCount = await _context.Enrollments
                .CountAsync(e => e.ClassId == classId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var absencesToday = await _context.AttendanceRecords
                .CountAsync(a => a.ClassId == classId && a.AbsenceDate == today);

            var upcomingExams = await _context.Exams
                .Include(e => e.Subject)
                .Where(e => e.Subject.ClassId == classId && e.ExamDate >= today)
                .OrderBy(e => e.ExamDate)
                .Take(5)
                .Select(e => new DashboardExamDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    SubjectName = e.Subject.Name,
                    ExamDate = e.ExamDate
                })
                .ToListAsync();

            var readIds = await _context.AnnouncementReadStatuses
                .Where(rs => rs.UserId == userId)
                .Select(rs => rs.AnnouncementId)
                .ToListAsync();

            var unreadAnnouncements = await _context.Announcements
                .CountAsync(a =>
                    !readIds.Contains(a.Id) &&
                    (a.Scope == AnnouncementScope.SchoolWide ||
                     (a.Scope == AnnouncementScope.ClassOnly && a.ClassId == classId)));

            var recentAnnouncements = await _context.Announcements
                .Where(a => a.Scope == AnnouncementScope.SchoolWide ||
                           (a.Scope == AnnouncementScope.ClassOnly && a.ClassId == classId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new DashboardAnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    CreatedAt = a.CreatedAt,
                    IsRead = readIds.Contains(a.Id)
                })
                .ToListAsync();

            return new BaseResponse<TeacherDashboardDto>
            {
                Success = true,
                Message = "Dashboard retrieved",
                StatusCode = 200,
                Data = new TeacherDashboardDto
                {
                    ClassStudentCount = classStudentCount,
                    AbsencesTodayInClass = absencesToday,
                    UpcomingExams = upcomingExams,
                    UnreadAnnouncements = unreadAnnouncements,
                    RecentAnnouncements = recentAnnouncements
                }
            };
        }

        public async Task<BaseResponse<StudentDashboardDto>> GetStudentDashboardAsync(Guid userId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return new BaseResponse<StudentDashboardDto>
                {
                    Success = true,
                    Message = "Dashboard retrieved",
                    StatusCode = 200,
                    Data = new StudentDashboardDto()
                };
            }

            var activeYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.IsActive);

            var totalAbsences = 0;
            if (activeYear != null)
            {
                totalAbsences = await _context.AttendanceRecords
                    .CountAsync(a => a.StudentId == student.Id &&
                                     a.AbsenceDate >= activeYear.StartDate &&
                                     a.AbsenceDate <= activeYear.EndDate);
            }

            // Calculate GPA
            var grades = await _context.Grades
                .Include(g => g.GradeCategory)
                    .ThenInclude(gc => gc.Subject)
                .Where(g => g.StudentId == student.Id)
                .ToListAsync();

            var subjectAverages = grades
                .GroupBy(g => g.GradeCategory.SubjectId)
                .Select(group => group.Sum(g => g.Score * g.GradeCategory.Weight / 100m))
                .ToList();

            var gpa = subjectAverages.Count > 0
                ? Math.Round(subjectAverages.Average(), 2)
                : 0m;

            // Get student's class
            var enrollment = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .OrderByDescending(e => e.EnrolledAt)
                .FirstOrDefaultAsync();

            var classId = enrollment?.ClassId;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            DashboardExamDto? nextExam = null;

            if (classId.HasValue)
            {
                nextExam = await _context.Exams
                    .Include(e => e.Subject)
                    .Where(e => e.Subject.ClassId == classId.Value && e.ExamDate >= today)
                    .OrderBy(e => e.ExamDate)
                    .Select(e => new DashboardExamDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        SubjectName = e.Subject.Name,
                        ExamDate = e.ExamDate
                    })
                    .FirstOrDefaultAsync();
            }

            var readIds = await _context.AnnouncementReadStatuses
                .Where(rs => rs.UserId == userId)
                .Select(rs => rs.AnnouncementId)
                .ToListAsync();

            var announcementQuery = _context.Announcements.AsQueryable();
            if (classId.HasValue)
            {
                announcementQuery = announcementQuery.Where(a =>
                    a.Scope == AnnouncementScope.SchoolWide ||
                    (a.Scope == AnnouncementScope.ClassOnly && a.ClassId == classId.Value));
            }
            else
            {
                announcementQuery = announcementQuery.Where(a => a.Scope == AnnouncementScope.SchoolWide);
            }

            var unreadAnnouncements = await announcementQuery
                .CountAsync(a => !readIds.Contains(a.Id));

            var recentAnnouncements = await announcementQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new DashboardAnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    CreatedAt = a.CreatedAt,
                    IsRead = readIds.Contains(a.Id)
                })
                .ToListAsync();

            return new BaseResponse<StudentDashboardDto>
            {
                Success = true,
                Message = "Dashboard retrieved",
                StatusCode = 200,
                Data = new StudentDashboardDto
                {
                    TotalAbsencesThisYear = totalAbsences,
                    Gpa = gpa,
                    NextExam = nextExam,
                    UnreadAnnouncements = unreadAnnouncements,
                    RecentAnnouncements = recentAnnouncements
                }
            };
        }
    }
}

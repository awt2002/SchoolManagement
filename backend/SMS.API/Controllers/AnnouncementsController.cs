using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Features.Announcements.DTOs;
using SMS.Application.Interfaces;
using SMS.Infrastructure.Data;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/announcements")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly AppDbContext _context;

        public AnnouncementsController(IAnnouncementService announcementService, AppDbContext context)
        {
            _announcementService = announcementService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageSize > 100) pageSize = 100;
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Student";
            var classId = await GetUserClassIdAsync(userId, userRole);

            var result = await _announcementService.GetAnnouncementsAsync(userId, userRole, classId, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _announcementService.CreateAnnouncementAsync(dto, userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _announcementService.GetAnnouncementByIdAsync(id, userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/read")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _announcementService.MarkAsReadAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Student";
            var classId = await GetUserClassIdAsync(userId, userRole);

            var result = await _announcementService.GetUnreadCountAsync(userId, userRole, classId);
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && Guid.TryParse(claim.Value, out var id))
            {
                return id;
            }
            return Guid.Empty;
        }

        private async Task<Guid?> GetUserClassIdAsync(Guid userId, string role)
        {
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == userId);
                return teacher?.ClassId;
            }
            else if (role == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                if (student != null)
                {
                    var enrollment = await _context.Enrollments
                        .Where(e => e.StudentId == student.Id)
                        .OrderByDescending(e => e.EnrolledAt)
                        .FirstOrDefaultAsync();
                    return enrollment?.ClassId;
                }
            }
            return null;
        }
    }
}

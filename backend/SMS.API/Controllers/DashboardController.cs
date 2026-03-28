using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Common;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetCurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Student";

            if (role == "Admin")
            {
                var result = await _dashboardService.GetAdminDashboardAsync(userId);
                return Ok(result);
            }
            else if (role == "Teacher")
            {
                var result = await _dashboardService.GetTeacherDashboardAsync(userId);
                return Ok(result);
            }
            else
            {
                var result = await _dashboardService.GetStudentDashboardAsync(userId);
                return Ok(result);
            }
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
    }
}

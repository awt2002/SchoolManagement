using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Common;
using SMS.Application.Features.Attendance.Commands;
using SMS.Application.Features.Attendance.Queries;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/attendance")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly GetAttendanceQueryHandler _getAttendanceQueryHandler;
        private readonly CreateAttendanceCommandHandler _createAttendanceCommandHandler;
        private readonly DeleteAttendanceCommandHandler _deleteAttendanceCommandHandler;
        private readonly GetStudentAttendanceSummaryQueryHandler _getStudentAttendanceSummaryQueryHandler;
        private readonly IStudentService _studentService;

        public AttendanceController(
            GetAttendanceQueryHandler getAttendanceQueryHandler,
            CreateAttendanceCommandHandler createAttendanceCommandHandler,
            DeleteAttendanceCommandHandler deleteAttendanceCommandHandler,
            GetStudentAttendanceSummaryQueryHandler getStudentAttendanceSummaryQueryHandler,
            IStudentService studentService)
        {
            _getAttendanceQueryHandler = getAttendanceQueryHandler;
            _createAttendanceCommandHandler = createAttendanceCommandHandler;
            _deleteAttendanceCommandHandler = deleteAttendanceCommandHandler;
            _getStudentAttendanceSummaryQueryHandler = getStudentAttendanceSummaryQueryHandler;
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? classId = null,
            [FromQuery] Guid? studentId = null,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null)
        {
            var result = await _getAttendanceQueryHandler.HandleAsync(new GetAttendanceQuery
            {
                ClassId = classId,
                StudentId = studentId,
                From = from,
                To = to
            });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceCommand command)
        {
            var userId = GetCurrentUserId();
            var result = await _createAttendanceCommandHandler.HandleAsync(command, userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteAttendanceCommandHandler.HandleAsync(new DeleteAttendanceCommand
            {
                Id = id
            });

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("student/{studentId}/summary")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetStudentSummary(
            Guid studentId,
            [FromQuery] Guid? academicYearId = null)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Student")
            {
                var myProfile = await _studentService.GetStudentByUserIdAsync(GetCurrentUserId());
                if (!myProfile.Success || myProfile.Data == null)
                {
                    return StatusCode(myProfile.StatusCode, myProfile);
                }

                if (myProfile.Data.Id != studentId)
                {
                    return StatusCode(403, new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Forbidden",
                        StatusCode = 403
                    });
                }
            }

            var result = await _getStudentAttendanceSummaryQueryHandler.HandleAsync(new GetStudentAttendanceSummaryQuery
            {
                StudentId = studentId,
                AcademicYearId = academicYearId
            });
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
    }
}

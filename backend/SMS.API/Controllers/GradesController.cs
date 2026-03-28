using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Common;
using SMS.Application.Features.Grades.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/grades")]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly IValidator<CreateGradeDto> _createGradeValidator;
        private readonly IStudentService _studentService;

        public GradesController(
            IGradeService gradeService,
            IValidator<CreateGradeDto> createGradeValidator,
            IStudentService studentService)
        {
            _gradeService = gradeService;
            _createGradeValidator = createGradeValidator;
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? subjectId = null,
            [FromQuery] Guid? studentId = null,
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

                if (studentId.HasValue && studentId.Value != myProfile.Data.Id)
                {
                    return StatusCode(403, new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Forbidden",
                        StatusCode = 403
                    });
                }

                studentId = myProfile.Data.Id;
            }

            var result = await _gradeService.GetGradesAsync(subjectId, studentId, academicYearId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create([FromBody] CreateGradeDto dto)
        {
            var validationResult = await _createGradeValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    StatusCode = 400,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetCurrentUserId();
            var result = await _gradeService.CreateOrUpdateGradeAsync(dto, userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(result.StatusCode == 201 ? 201 : 200, result);
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? studentId = null,
            [FromQuery] Guid? classId = null,
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

                if (studentId.HasValue && studentId.Value != myProfile.Data.Id)
                {
                    return StatusCode(403, new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Forbidden",
                        StatusCode = 403
                    });
                }

                studentId = myProfile.Data.Id;
            }

            var result = await _gradeService.GetGradeSummaryAsync(studentId, classId, academicYearId);
            return Ok(result);
        }

        [HttpGet("audit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuditLog(
            [FromQuery] Guid? studentId = null,
            [FromQuery] Guid? subjectId = null,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageSize > 100) pageSize = 100;
            var result = await _gradeService.GetAuditLogAsync(studentId, subjectId, from, to, page, pageSize);
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

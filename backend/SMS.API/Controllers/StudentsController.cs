using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Common;
using SMS.Application.Features.Students.Commands;
using SMS.Application.Features.Students.Queries;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/students")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly CreateStudentCommandHandler _createStudentCommandHandler;
        private readonly UpdateStudentCommandHandler _updateStudentCommandHandler;
        private readonly GetStudentsQueryHandler _getStudentsQueryHandler;
        private readonly GetStudentByIdQueryHandler _getStudentByIdQueryHandler;

        public StudentsController(
            IStudentService studentService,
            CreateStudentCommandHandler createStudentCommandHandler,
            UpdateStudentCommandHandler updateStudentCommandHandler,
            GetStudentsQueryHandler getStudentsQueryHandler,
            GetStudentByIdQueryHandler getStudentByIdQueryHandler)
        {
            _studentService = studentService;
            _createStudentCommandHandler = createStudentCommandHandler;
            _updateStudentCommandHandler = updateStudentCommandHandler;
            _getStudentsQueryHandler = getStudentsQueryHandler;
            _getStudentByIdQueryHandler = getStudentByIdQueryHandler;
        }

        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var result = await _studentService.GetStudentByUserIdAsync(userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? gradeLevel = null,
            [FromQuery] Guid? classId = null,
            [FromQuery] Guid? academicYearId = null,
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool inactiveOnly = false)
        {
            if (pageSize > 100) pageSize = 100;
            var result = await _getStudentsQueryHandler.HandleAsync(new GetStudentsQuery
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                GradeLevel = gradeLevel,
                ClassId = classId,
                AcademicYearId = academicYearId,
                IncludeInactive = includeInactive,
                InactiveOnly = inactiveOnly
            });

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStudentCommand command)
        {
            var result = await _createStudentCommandHandler.HandleAsync(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Students can only view their own profile
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Student")
            {
                var userId = GetCurrentUserId();
                // We need to check if this student belongs to the user
                var studentResult = await _getStudentByIdQueryHandler.HandleAsync(new GetStudentByIdQuery { Id = id });
                if (studentResult.Data != null && studentResult.Data.UserId != userId)
                {
                    return StatusCode(403, new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Forbidden",
                        StatusCode = 403
                    });
                }
                return Ok(studentResult);
            }

            var result = await _getStudentByIdQueryHandler.HandleAsync(new GetStudentByIdQuery { Id = id });

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentCommand command)
        {
            if (command.Id != id)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Route id and payload id must match",
                    StatusCode = 400
                });
            }

            var result = await _updateStudentCommandHandler.HandleAsync(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _studentService.DeleteStudentAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactivate(Guid id)
        {
            var result = await _studentService.ReactivateStudentAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/photo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "No file uploaded",
                    StatusCode = 400
                });
            }

            if (file.Length > 2 * 1024 * 1024)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "File size exceeds 2 MB limit",
                    StatusCode = 400
                });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Only JPG and PNG files are accepted",
                    StatusCode = 400
                });
            }

            var result = await _studentService.UploadPhotoAsync(id, file.FileName, file.OpenReadStream());

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/enrollments")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetEnrollments(Guid id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Student")
            {
                var userId = GetCurrentUserId();
                var studentResult = await _getStudentByIdQueryHandler.HandleAsync(new GetStudentByIdQuery { Id = id });
                if (!studentResult.Success || studentResult.Data == null)
                {
                    return StatusCode(studentResult.StatusCode, studentResult);
                }

                if (studentResult.Data.UserId != userId)
                {
                    return StatusCode(403, new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Forbidden",
                        StatusCode = 403
                    });
                }
            }

            var result = await _studentService.GetEnrollmentsAsync(id);
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/teachers")]
    [Authorize(Roles = "Admin,Teacher")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool inactiveOnly = false)
        {
            if (pageSize > 100) pageSize = 100;
            var result = await _teacherService.GetAllTeachersAsync(page, pageSize, search, includeInactive, inactiveOnly);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMe()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await _teacherService.GetTeacherByUserIdAsync(userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(dto.Username) || dto.Username.Length < 4 || dto.Username.Length > 20)
            {
                errors.Add("Username must be 4-20 characters and contain only letters and numbers.");
            }

            if (!System.Net.Mail.MailAddress.TryCreate(dto.Email, out _))
            {
                errors.Add("Please enter a valid email address.");
            }

            if (errors.Count > 0)
            {
                return BadRequest(new SMS.Application.Common.BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors,
                    StatusCode = 400
                });
            }

            var result = await _teacherService.CreateTeacherAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _teacherService.GetTeacherByIdAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherDto dto)
        {
            var result = await _teacherService.UpdateTeacherAsync(id, dto);

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
            var result = await _teacherService.DeleteTeacherAsync(id);

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
            var result = await _teacherService.ReactivateTeacherAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}

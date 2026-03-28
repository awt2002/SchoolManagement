using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/exams")]
    [Authorize]
    public class ExamsController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamsController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? subjectId = null,
            [FromQuery] Guid? classId = null,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null)
        {
            var result = await _examService.GetExamsAsync(subjectId, classId, from, to);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create([FromBody] CreateExamDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _examService.CreateExamAsync(dto, userId);

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
            var result = await _examService.GetExamByIdAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExamDto dto)
        {
            var result = await _examService.UpdateExamAsync(id, dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/results")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetResults(Guid id)
        {
            var result = await _examService.GetExamResultsAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/results")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateResults(Guid id, [FromBody] BulkExamResultDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _examService.CreateExamResultsAsync(id, dto, userId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpGet("{id}/results/{studentId}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetStudentResult(Guid id, Guid studentId)
        {
            var result = await _examService.GetStudentExamResultAsync(id, studentId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

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

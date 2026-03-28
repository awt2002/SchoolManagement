using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Subjects.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/subjects")]
    [Authorize]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetAll([FromQuery] Guid? classId = null)
        {
            var result = await _subjectService.GetSubjectsAsync(classId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
        {
            var result = await _subjectService.CreateSubjectAsync(dto);

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
            var result = await _subjectService.GetSubjectByIdAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectDto dto)
        {
            var result = await _subjectService.UpdateSubjectAsync(id, dto);

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
            var result = await _subjectService.DeleteSubjectAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("{subjectId}/categories")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetCategories(Guid subjectId)
        {
            var result = await _subjectService.GetCategoriesAsync(subjectId);
            return Ok(result);
        }

        [HttpPost("{subjectId}/categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(Guid subjectId, [FromBody] CreateGradeCategoryDto dto)
        {
            var result = await _subjectService.CreateCategoryAsync(subjectId, dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpPut("{subjectId}/categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid subjectId, Guid id, [FromBody] UpdateGradeCategoryDto dto)
        {
            var result = await _subjectService.UpdateCategoryAsync(subjectId, id, dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpDelete("{subjectId}/categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid subjectId, Guid id)
        {
            var result = await _subjectService.DeleteCategoryAsync(subjectId, id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}

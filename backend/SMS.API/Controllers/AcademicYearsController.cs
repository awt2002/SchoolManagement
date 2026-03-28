using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.AcademicYears.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/academic-years")]
    [Authorize(Roles = "Admin")]
    public class AcademicYearsController : ControllerBase
    {
        private readonly IAcademicYearService _academicYearService;

        public AcademicYearsController(IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _academicYearService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAcademicYearDto dto)
        {
            var result = await _academicYearService.CreateAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return StatusCode(201, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAcademicYearDto dto)
        {
            var result = await _academicYearService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _academicYearService.ActivateAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}

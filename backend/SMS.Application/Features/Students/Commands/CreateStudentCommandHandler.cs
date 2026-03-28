using FluentValidation;
using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Students.Commands
{
    public class CreateStudentCommandHandler
    {
        private readonly IStudentService _studentService;
        private readonly IValidator<CreateStudentDto> _validator;

        public CreateStudentCommandHandler(IStudentService studentService, IValidator<CreateStudentDto> validator)
        {
            _studentService = studentService;
            _validator = validator;
        }

        public async Task<BaseResponse<StudentDetailDto>> HandleAsync(CreateStudentCommand command)
        {
            var dto = command.ToDto();
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return new BaseResponse<StudentDetailDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    StatusCode = 400,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
            }

            return await _studentService.CreateStudentAsync(dto);
        }
    }
}

using SMS.Application.Common;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Features.Students.Validators;
using SMS.Application.Interfaces;

namespace SMS.Application.Features.Students.Commands
{
    public class UpdateStudentCommandHandler
    {
        private readonly IStudentService _studentService;
        private readonly UpdateStudentCommandValidator _validator;

        public UpdateStudentCommandHandler(IStudentService studentService, UpdateStudentCommandValidator validator)
        {
            _studentService = studentService;
            _validator = validator;
        }

        public async Task<BaseResponse<StudentDetailDto>> HandleAsync(UpdateStudentCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
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

            var existingResult = await _studentService.GetStudentByIdAsync(command.Id);
            if (!existingResult.Success || existingResult.Data == null)
            {
                return existingResult;
            }

            var updateDto = new UpdateStudentDto
            {
                FullName = command.FullName,
                ParentEmail = command.ParentEmail ?? existingResult.Data.ParentEmail,
                DateOfBirth = existingResult.Data.DateOfBirth,
                Address = existingResult.Data.Address,
                ClassId = existingResult.Data.CurrentClassId ?? Guid.Empty,
                ParentName = existingResult.Data.ParentName,
                ParentPhone = existingResult.Data.ParentPhone
            };

            return await _studentService.UpdateStudentAsync(command.Id, updateDto);
        }
    }
}

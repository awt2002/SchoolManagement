using FluentValidation;
using SMS.Application.Features.Students.DTOs;

namespace SMS.Application.Features.Students.Validators
{
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(250).WithMessage("Address must not exceed 250 characters.");

            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth cannot be in the future.");

            RuleFor(x => x.ParentEmail)
                .EmailAddress().WithMessage("Parent email must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.ParentEmail));

            RuleFor(x => x)
                .Must(x => x.ClassId.HasValue == x.AcademicYearId.HasValue)
                .WithMessage("Class and academic year must both be provided when enrolling a student.");
        }
    }
}

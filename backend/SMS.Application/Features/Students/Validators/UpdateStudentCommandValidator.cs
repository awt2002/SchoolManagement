using FluentValidation;
using SMS.Application.Features.Students.Commands;

namespace SMS.Application.Features.Students.Validators
{
    public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.");

            RuleFor(x => x.ParentEmail)
                .EmailAddress().WithMessage("ParentEmail must be a valid email format.")
                .When(x => !string.IsNullOrWhiteSpace(x.ParentEmail));
        }
    }
}

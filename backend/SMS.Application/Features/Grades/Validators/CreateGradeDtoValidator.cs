using FluentValidation;
using SMS.Application.Features.Grades.DTOs;

namespace SMS.Application.Features.Grades.Validators
{
    public class CreateGradeDtoValidator : AbstractValidator<CreateGradeDto>
    {
        public CreateGradeDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .NotEmpty().WithMessage("StudentId is required.");

            RuleFor(x => x.GradeCategoryId)
                .NotEmpty().WithMessage("GradeCategoryId is required.");

            RuleFor(x => x.Score)
                .InclusiveBetween(0, 100).WithMessage("Score must be between 0 and 100.");
        }
    }
}

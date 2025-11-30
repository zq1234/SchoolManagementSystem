using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class UpdateGradeDtoValidator : AbstractValidator<UpdateGradeDto>
    {
        public UpdateGradeDtoValidator()
        {
            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative.");

            RuleFor(x => x.TotalScore)
                .GreaterThan(0).WithMessage("Total score must be greater than 0.");

            RuleFor(x => x.Comments)
                .MaximumLength(500).WithMessage("Comments cannot exceed 500 characters.");
        }
    }
}
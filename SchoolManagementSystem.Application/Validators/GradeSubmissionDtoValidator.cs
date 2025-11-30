using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class GradeSubmissionDtoValidator : AbstractValidator<GradeSubmissionDto>
    {
        public GradeSubmissionDtoValidator()
        {
            RuleFor(x => x.Grade)
                .NotEmpty().WithMessage("Grade is required.")
                .MaximumLength(10).WithMessage("Grade cannot exceed 10 characters.");

            RuleFor(x => x.Remarks)
                .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
        }
    }
}
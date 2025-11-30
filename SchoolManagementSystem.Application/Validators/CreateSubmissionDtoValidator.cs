using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class CreateSubmissionDtoValidator : AbstractValidator<CreateSubmissionDto>
    {
        public CreateSubmissionDtoValidator()
        {
            RuleFor(x => x.AssignmentId)
                .GreaterThan(0).WithMessage("Assignment ID is required.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.");

            RuleFor(x => x.Remarks)
                .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
        }
    }
}
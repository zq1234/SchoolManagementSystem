using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class CreateGradeDtoValidator : AbstractValidator<CreateGradeDto>
    {
        public CreateGradeDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("Student ID is required.");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID is required.");

            RuleFor(x => x.EnrollmentId)
                .GreaterThan(0).WithMessage("Enrollment ID is required.");

            RuleFor(x => x.AssessmentType)
                .NotEmpty().WithMessage("Assessment type is required.")
                .MaximumLength(50).WithMessage("Assessment type cannot exceed 50 characters.");

            RuleFor(x => x.AssessmentName)
                .NotEmpty().WithMessage("Assessment name is required.")
                .MaximumLength(100).WithMessage("Assessment name cannot exceed 100 characters.");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative.");

            RuleFor(x => x.TotalScore)
                .GreaterThan(0).WithMessage("Total score must be greater than 0.");

            RuleFor(x => x.AssessmentDate)
                .NotEmpty().WithMessage("Assessment date is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Assessment date cannot be in the future.");

            RuleFor(x => x.Comments)
                .MaximumLength(500).WithMessage("Comments cannot exceed 500 characters.");
        }
    }
}
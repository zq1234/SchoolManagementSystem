using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class CreateClassDtoValidator : AbstractValidator<CreateClassDto>
    {
        public CreateClassDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Class name is required.")
                .MaximumLength(50).WithMessage("Class name cannot exceed 50 characters.");

            RuleFor(x => x.Section)
                .NotEmpty().WithMessage("Section is required.")
                .MaximumLength(10).WithMessage("Section cannot exceed 10 characters.");

            RuleFor(x => x.Semester)
                .NotEmpty().WithMessage("Semester is required.")
                .MaximumLength(20).WithMessage("Semester cannot exceed 20 characters.");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID is required.");

            RuleFor(x => x.Room)
                .MaximumLength(20).WithMessage("Room cannot exceed 20 characters.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

            RuleFor(x => x.DaysOfWeek)
                .MaximumLength(50).WithMessage("Days of week cannot exceed 50 characters.");
        }
    }
}
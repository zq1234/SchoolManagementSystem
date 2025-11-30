using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class CreateAssignmentDtoValidator : AbstractValidator<CreateAssignmentDto>
    {
        public CreateAssignmentDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required.")
                .GreaterThan(DateTime.Now).WithMessage("Due date must be in the future.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0).WithMessage("Class ID is required.");
        }
    }
}
using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
    {
        public CreateDepartmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.HeadOfDepartmentId)
                .GreaterThan(0).WithMessage("Head of Department ID is required.");
        }
    }
}
using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Now.AddYears(-5)).WithMessage("Student must be at least 5 years old.")
                .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Invalid date of birth.");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");
        }
    }
}
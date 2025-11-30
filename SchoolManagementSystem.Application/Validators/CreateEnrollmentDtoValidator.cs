using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class CreateEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
    {
        public CreateEnrollmentDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("Student ID is required.");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID is required.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0).WithMessage("Class ID is required.");
        }
    }
}
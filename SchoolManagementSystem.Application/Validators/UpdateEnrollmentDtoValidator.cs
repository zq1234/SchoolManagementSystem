using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class UpdateEnrollmentDtoValidator : AbstractValidator<UpdateEnrollmentDto>
    {
        public UpdateEnrollmentDtoValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0).WithMessage("Class ID is required.");
        }
    }
}
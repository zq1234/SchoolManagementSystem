using FluentValidation;

namespace SchoolManagementSystem.Application.DTOs
{
    public class CreateStudentClassDtoValidator : AbstractValidator<CreateStudentClassDto>
    {
        public CreateStudentClassDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("Student ID must be greater than 0");

            RuleFor(x => x.ClassId)
                .GreaterThan(0).WithMessage("Class ID must be greater than 0");
        }
    }

    public class BulkEnrollmentDtoValidator : AbstractValidator<BulkEnrollmentDto>
    {
        public BulkEnrollmentDtoValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0).WithMessage("Class ID must be greater than 0");

            RuleFor(x => x.StudentIds)
                .NotEmpty().WithMessage("Student IDs are required")
                .Must(ids => ids != null && ids.Count >= 1).WithMessage("At least one student ID is required");

            RuleForEach(x => x.StudentIds)
                .GreaterThan(0).WithMessage("Each student ID must be greater than 0");
        }
    }
}
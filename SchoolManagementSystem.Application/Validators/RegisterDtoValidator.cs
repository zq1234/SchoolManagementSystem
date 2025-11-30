using FluentValidation;
using SchoolManagementSystem.Core.DTOs.Authentication;

namespace SchoolManagementSystem.Application.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            //RuleFor(x => x.Role)
            //    .NotEmpty().WithMessage("Role is required.")
            //    .Must(BeAValidRole).WithMessage("Invalid role specified.");

            //When(x => x.Role == "Student", () => {
            //    RuleFor(x => x.StudentId).NotEmpty().WithMessage("Student ID is required for students.");
            //});

            //When(x => x.Role == "Teacher", () => {
            //    RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee ID is required for teachers.");
            //    RuleFor(x => x.Department).NotEmpty().WithMessage("Department is required for teachers.");
            //});
        }

        private static bool BeAValidRole(string role)
        {
            return role switch
            {
                "Admin" or "Teacher" or "Student" => true,
                _ => false
            };
        }
    }
}
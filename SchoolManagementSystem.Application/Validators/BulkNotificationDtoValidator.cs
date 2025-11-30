using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class BulkNotificationDtoValidator : AbstractValidator<BulkNotificationDto>
    {
        public BulkNotificationDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Message)
                .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters.");

            RuleFor(x => x.RecipientRole)
                .NotEmpty().WithMessage("Recipient role is required.")
                .Must(BeAValidRole).WithMessage("Invalid recipient role.");

            RuleFor(x => x.RecipientIds)
                .NotEmpty().WithMessage("At least one recipient ID is required.")
                .Must(ids => ids != null && ids.Any()).WithMessage("At least one recipient ID is required.");
        }

        private bool BeAValidRole(string role)
        {
            return role == "All" || role == "Student" || role == "Teacher" || role == "Admin";
        }
    }
}
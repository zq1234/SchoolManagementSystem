using FluentValidation;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Validators
{
    public class UpdateAttendanceDtoValidator : AbstractValidator<UpdateAttendanceDto>
    {
        public UpdateAttendanceDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(BeAValidStatus).WithMessage("Invalid attendance status.");

            RuleFor(x => x.Remarks)
                .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
        }

        private bool BeAValidStatus(string status)
        {
            return status == "Present" || status == "Absent" || status == "Late" || status == "Excused";
        }
    }
}
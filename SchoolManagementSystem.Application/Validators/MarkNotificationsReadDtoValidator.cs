using FluentValidation;
using SchoolManagementSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Validators
{
    public class MarkNotificationsReadDtoValidator : AbstractValidator<MarkNotificationsReadDto>
    {
        public MarkNotificationsReadDtoValidator()
        {
            RuleFor(x => x.NotificationIds)
                .NotEmpty().WithMessage("Notification IDs are required.")
                .Must(ids => ids != null && ids.Count >= 1).WithMessage("At least one notification ID is required.");
        }
    }
}


using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string RecipientRole { get; set; } = string.Empty; // ADDED: Admin/Teacher/Student/All
        public int? RecipientId { get; set; }  
        public bool IsRead { get; set; } = false;

        // ADD proper relationships:
        public string? RecipientUserId { get; set; }
        public virtual User? RecipientUser { get; set; }
    }
}

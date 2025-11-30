using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Enums
{
    public enum AssignmentStatus
    {
        Pending = 0,      // Not submitted yet
        Submitted = 1,    // Student submitted
        Graded = 2 ,      // Optional: Teacher evaluated
        Rejected = 3,       // Optional: Teacher rejected
        ReSubmitted = 4, // Optional: student can resubmitted
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs
{
    public class BulkEnrollmentRequestDto
    {
        public int ClassId { get; set; }
        public List<int> StudentIds { get; set; } = new();
    }

    public class BulkEnrollmentResultDto
    {
        public int TotalProcessed { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<string> FailureDetails { get; set; } = new();
        public bool HasFailures => Failed > 0;
    }
}

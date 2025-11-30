using SchoolManagementSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Entities
{

    public class Department : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; } = null;

        // FK to Teacher: Head of Department
        public int? HeadOfDepartmentId { get; set; }

        [ForeignKey(nameof(HeadOfDepartmentId))]
        public Teacher? HeadOfDepartment { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }

}

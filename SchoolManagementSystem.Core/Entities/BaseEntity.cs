using SchoolManagementSystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Entities
{
    public abstract class BaseEntity : ISoftDelete
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }

        public DateTime? UpdatedDate { get; set; }= DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public DateTime? DeletedDate { get; set; }= DateTime.UtcNow;
        public string? DeletedById { get; set; }
        public virtual User? DeletedBy { get; set; }


        // public virtual User? CreatedBy { get; set; }
        // public virtual User? UpdatedBy { get; set; }

    }

}

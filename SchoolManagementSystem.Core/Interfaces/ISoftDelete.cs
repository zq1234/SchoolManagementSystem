using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface ISoftDelete
    {
        bool IsActive { get; set; }
        DateTime? DeletedDate { get; set; }
        User? DeletedBy { get; set; }
    }
}

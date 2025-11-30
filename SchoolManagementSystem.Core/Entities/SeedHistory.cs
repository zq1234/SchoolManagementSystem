using System;

namespace SchoolManagementSystem.Core.Entities
{
    public class SeedHistory
    {
        public int Id { get; set; }
        public string SeedKey { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
    }
}

using SchoolManagementSystem.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Student> Students { get; }
        IRepository<Teacher> Teachers { get; }
        IRepository<Course> Courses { get; }
        IRepository<Class> Classes { get; }
        IRepository<Enrollment> Enrollments { get; }
        IRepository<Grade> Grades { get; }
        IRepository<Attendance> Attendances { get; }
        IRepository<Assignment> Assignments { get; }
        IRepository<Notification> Notifications { get; }
        IRepository<Department> Departments { get; }
        IRepository<Submission> Submissions { get; }
       // IRepository<StudentClass> StudentClasses { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

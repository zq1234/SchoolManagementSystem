using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;
using Serilog.Parsing;
using System;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IRepository<User>? _users;
        private IRepository<Student>? _students;
        private IRepository<Teacher>? _teachers;
        private IRepository<Course>? _courses;
        private IRepository<Class>? _classes;
        private IRepository<Enrollment>? _enrollments;
        private IRepository<Grade>? _grades;
        private IRepository<Attendance>? _attendances;
        private IRepository<Assignment>? _assignments;
        private IRepository<Notification>? _notifications;
        private IRepository<Department>? _departments;
        private IRepository<Submission>? _submission;
       // private IRepository<StudentClass>? _studentclasses;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Student> Students => _students ??= new Repository<Student>(_context);
        public IRepository<Teacher> Teachers => _teachers ??= new Repository<Teacher>(_context);
        public IRepository<Course> Courses => _courses ??= new Repository<Course>(_context);
        public IRepository<Class> Classes => _classes ??= new Repository<Class>(_context);
        public IRepository<Enrollment> Enrollments => _enrollments ??= new Repository<Enrollment>(_context);
        public IRepository<Grade> Grades => _grades ??= new Repository<Grade>(_context);
        public IRepository<Attendance> Attendances => _attendances ??= new Repository<Attendance>(_context);
        public IRepository<Assignment> Assignments => _assignments ??= new Repository<Assignment>(_context);
        public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);
        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public IRepository<Department> Departments => _departments ??= new Repository<Department>(_context);
        public IRepository<Submission> Submissions => _submission ??= new Repository<Submission>(_context);
       // public IRepository<StudentClass> StudentClasses => _studentclasses ??= new Repository<StudentClass>(_context);

        public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
        }


        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction in progress.");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();

            if (_context != null)
                await _context.DisposeAsync();
        }
    }
}

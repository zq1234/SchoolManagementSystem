using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Enums;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.API
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();

            // Create roles
            string[] roleNames = { UserRoles.SuperAdmin, UserRoles.Admin, UserRoles.Teacher, UserRoles.Student };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Created role: {roleName}");
                }
                else
                {
                    Console.WriteLine($"Role already exists: {roleName}");
                }
            }

            // Create super admin user
            var superAdminUser = await userManager.FindByEmailAsync("superadmin@sms.com");
            if (superAdminUser == null)
            {
                superAdminUser = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = "superadmin@sms.com",
                    Email = "superadmin@sms.com",
                    EmailConfirmed = true,
                    PhoneNumber = "+1234567890",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(superAdminUser, new[] { UserRoles.SuperAdmin, UserRoles.Admin });
                    Console.WriteLine("Created Super Admin user");
                }
                else
                {
                    Console.WriteLine($"Failed to create Super Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("Super Admin user already exists");
            }

            // Create admin user
            var adminUser = await userManager.FindByEmailAsync("admin@sms.com");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    UserName = "admin@sms.com",
                    Email = "admin@sms.com",
                    EmailConfirmed = true,
                    PhoneNumber = "+1234567891",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    Console.WriteLine("Created Admin user");
                }
                else
                {
                    Console.WriteLine($"Failed to create Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists");
            }

            // Create sample departments
            var departmentsToCreate = new[]
            {
                new { Name = "Computer Science", Description = "Computer Science and Engineering Department" },
                new { Name = "Mathematics", Description = "Mathematics and Statistics Department" },
                new { Name = "Physics", Description = "Physics and Astronomy Department" },
                new { Name = "English", Description = "English Literature and Language Department" }
            };

            foreach (var dept in departmentsToCreate)
            {
                var existingDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == dept.Name);
                if (existingDept == null)
                {
                    var department = new Department
                    {
                        Name = dept.Name,
                        Description = dept.Description,
                        CreatedDate = DateTime.UtcNow
                    };
                    await context.Departments.AddAsync(department);
                    Console.WriteLine($"Created department: {dept.Name}");
                }
                else
                {
                    Console.WriteLine($"Department already exists: {dept.Name}");
                }
            }
            await context.SaveChangesAsync();

            // Create sample teachers
            var teachersData = new[]
            {
                new { Email = "john.smith@sms.com", FirstName = "John", LastName = "Smith", EmployeeId = "TCH001", Department = "Computer Science", Qualification = "M.Sc. Computer Science", Salary = 55000 },
                new { Email = "sarah.jones@sms.com", FirstName = "Sarah", LastName = "Jones", EmployeeId = "TCH002", Department = "Mathematics", Qualification = "Ph.D. Mathematics", Salary = 60000 },
                new { Email = "michael.brown@sms.com", FirstName = "Michael", LastName = "Brown", EmployeeId = "TCH003", Department = "Physics", Qualification = "Ph.D. Physics", Salary = 58000 },
                new { Email = "emily.wilson@sms.com", FirstName = "Emily", LastName = "Wilson", EmployeeId = "TCH004", Department = "English", Qualification = "M.A. English Literature", Salary = 52000 }
            };

            foreach (var teacherData in teachersData)
            {
                var teacherUser = await userManager.FindByEmailAsync(teacherData.Email);
                if (teacherUser == null)
                {
                    teacherUser = new User
                    {
                        FirstName = teacherData.FirstName,
                        LastName = teacherData.LastName,
                        UserName = teacherData.Email,
                        Email = teacherData.Email,
                        EmailConfirmed = true,
                        PhoneNumber = "+1234567892",
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(teacherUser, "Teacher123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(teacherUser, UserRoles.Teacher);

                        var existingTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == teacherData.EmployeeId);
                        if (existingTeacher == null)
                        {
                            var teacher = new Teacher
                            {
                                UserId = teacherUser.Id,
                                EmployeeId = teacherData.EmployeeId,
                                Department = teacherData.Department,
                                Qualification = teacherData.Qualification,
                                HireDate = DateTime.UtcNow.AddYears(-new Random().Next(1, 5)),
                                Salary = teacherData.Salary,
                                CreatedDate = DateTime.UtcNow
                            };

                            context.Teachers.Add(teacher);
                            Console.WriteLine($"Created teacher: {teacherData.FirstName} {teacherData.LastName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create teacher user {teacherData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"Teacher user already exists: {teacherData.Email}");
                }
            }

            await context.SaveChangesAsync();

            // Assign Head of Departments only if not already assigned
            var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Computer Science");
            var mathDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Mathematics");
            var physicsDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Physics");
            var englishDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "English");

            var csTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == "TCH001");
            var mathTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == "TCH002");
            var physicsTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == "TCH003");
            var englishTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == "TCH004");

            if (csDept != null && csTeacher != null && csDept.HeadOfDepartmentId == null)
            {
                csDept.HeadOfDepartmentId = csTeacher.Id;
                Console.WriteLine("Assigned Head of Department for Computer Science");
            }

            if (mathDept != null && mathTeacher != null && mathDept.HeadOfDepartmentId == null)
            {
                mathDept.HeadOfDepartmentId = mathTeacher.Id;
                Console.WriteLine("Assigned Head of Department for Mathematics");
            }

            if (physicsDept != null && physicsTeacher != null && physicsDept.HeadOfDepartmentId == null)
            {
                physicsDept.HeadOfDepartmentId = physicsTeacher.Id;
                Console.WriteLine("Assigned Head of Department for Physics");
            }

            if (englishDept != null && englishTeacher != null && englishDept.HeadOfDepartmentId == null)
            {
                englishDept.HeadOfDepartmentId = englishTeacher.Id;
                Console.WriteLine("Assigned Head of Department for English");
            }

            await context.SaveChangesAsync();

            // Create sample courses only if they don't exist
            var coursesToCreate = new[]
            {
                new { Code = "CS101", Name = "Introduction to Computer Science", Description = "Basic concepts of computer science and programming", Credits = 3, Duration = 45, Fee = 1000, Department = "Computer Science", TeacherEmployeeId = "TCH001" },
                new { Code = "CS201", Name = "Data Structures", Description = "Advanced data structures and algorithms", Credits = 4, Duration = 60, Fee = 1200, Department = "Computer Science", TeacherEmployeeId = "TCH001" },
                new { Code = "MATH101", Name = "Calculus I", Description = "Differential and integral calculus", Credits = 4, Duration = 60, Fee = 1200, Department = "Mathematics", TeacherEmployeeId = "TCH002" },
                new { Code = "MATH201", Name = "Linear Algebra", Description = "Vector spaces and linear transformations", Credits = 3, Duration = 45, Fee = 1100, Department = "Mathematics", TeacherEmployeeId = "TCH002" },
                new { Code = "PHY101", Name = "Physics I", Description = "Classical mechanics and thermodynamics", Credits = 4, Duration = 60, Fee = 1200, Department = "Physics", TeacherEmployeeId = "TCH003" },
                new { Code = "PHY201", Name = "Modern Physics", Description = "Quantum mechanics and relativity", Credits = 4, Duration = 60, Fee = 1300, Department = "Physics", TeacherEmployeeId = "TCH003" },
                new { Code = "ENG101", Name = "English Composition", Description = "Basic writing and communication skills", Credits = 3, Duration = 45, Fee = 900, Department = "English", TeacherEmployeeId = "TCH004" },
                new { Code = "ENG201", Name = "British Literature", Description = "Survey of British literature through ages", Credits = 3, Duration = 45, Fee = 950, Department = "English", TeacherEmployeeId = "TCH004" }
            };

            foreach (var courseData in coursesToCreate)
            {
                var existingCourse = await context.Courses.FirstOrDefaultAsync(c => c.Code == courseData.Code);
                if (existingCourse == null)
                {
                    var department = await context.Departments.FirstOrDefaultAsync(d => d.Name == courseData.Department);
                    var teacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == courseData.TeacherEmployeeId);

                    var course = new Course
                    {
                        Code = courseData.Code,
                        Name = courseData.Name,
                        Description = courseData.Description,
                        Credits = courseData.Credits,
                        Duration = courseData.Duration,
                        Fee = courseData.Fee,
                        DepartmentId = department?.Id,
                        TeacherId = teacher?.Id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    await context.Courses.AddAsync(course);
                    Console.WriteLine($"Created course: {courseData.Code} - {courseData.Name}");
                }
                else
                {
                    Console.WriteLine($"Course already exists: {courseData.Code}");
                }
            }
            await context.SaveChangesAsync();

            // Create sample classes only if they don't exist
            var classesToCreate = new[]
            {
                new { Name = "CS101", Section = "A", CourseCode = "CS101", TeacherEmployeeId = "TCH001", Room = "CS-101", Semester = "Fall 2024" },
                new { Name = "MATH101", Section = "B", CourseCode = "MATH101", TeacherEmployeeId = "TCH002", Room = "MATH-201", Semester = "Fall 2024" },
                new { Name = "PHY101", Section = "A", CourseCode = "PHY101", TeacherEmployeeId = "TCH003", Room = "PHY-301", Semester = "Fall 2024" },
                new { Name = "ENG101", Section = "C", CourseCode = "ENG101", TeacherEmployeeId = "TCH004", Room = "ENG-101", Semester = "Fall 2024" }
            };

            foreach (var classData in classesToCreate)
            {
                var existingClass = await context.Classes
                    .FirstOrDefaultAsync(c => c.Name == classData.Name && c.Section == classData.Section);

                if (existingClass == null)
                {
                    var course = await context.Courses.FirstOrDefaultAsync(c => c.Code == classData.CourseCode);
                    var teacher = await context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == classData.TeacherEmployeeId);

                    if (course != null && teacher != null)
                    {
                        var classEntity = new Class
                        {
                            Name = classData.Name,
                            Section = classData.Section,
                            CourseId = course.Id,
                            TeacherId = teacher.Id,
                            Room = classData.Room,
                            Semester = classData.Semester,
                            StartDate = DateTime.UtcNow.AddMonths(-2),
                            EndDate = DateTime.UtcNow.AddMonths(4),
                            DaysOfWeek = classData.Name.StartsWith("CS") || classData.Name.StartsWith("PHY") ? "MWF" : "TTH",
                            CreatedDate = DateTime.UtcNow
                        };

                        await context.Classes.AddAsync(classEntity);
                        Console.WriteLine($"Created class: {classData.Name} - {classData.Section}");
                    }
                }
                else
                {
                    Console.WriteLine($"Class already exists: {classData.Name} - {classData.Section}");
                }
            }
            await context.SaveChangesAsync();

            // Create sample students
            var studentsData = new[]
            {
                new { Email = "alice.johnson@sms.com", FirstName = "Alice", LastName = "Johnson", StudentId = "STU001", DateOfBirth = new DateTime(2000, 5, 15) },
                new { Email = "bob.williams@sms.com", FirstName = "Bob", LastName = "Williams", StudentId = "STU002", DateOfBirth = new DateTime(2001, 3, 22) },
                new { Email = "carol.davis@sms.com", FirstName = "Carol", LastName = "Davis", StudentId = "STU003", DateOfBirth = new DateTime(2000, 8, 10) },
                new { Email = "david.miller@sms.com", FirstName = "David", LastName = "Miller", StudentId = "STU004", DateOfBirth = new DateTime(2001, 1, 30) },
                new { Email = "eva.garcia@sms.com", FirstName = "Eva", LastName = "Garcia", StudentId = "STU005", DateOfBirth = new DateTime(2000, 11, 5) }
            };

            foreach (var studentData in studentsData)
            {
                var studentUser = await userManager.FindByEmailAsync(studentData.Email);
                if (studentUser == null)
                {
                    studentUser = new User
                    {
                        FirstName = studentData.FirstName,
                        LastName = studentData.LastName,
                        UserName = studentData.Email,
                        Email = studentData.Email,
                        EmailConfirmed = true,
                        PhoneNumber = "+1234567893",
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                        PhotoUrl = "/uploads/students/photos/default-student.jpg"
                    };

                    var result = await userManager.CreateAsync(studentUser, "Student123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(studentUser, UserRoles.Student);

                        var existingStudent = await context.Students.FirstOrDefaultAsync(s => s.StudentId == studentData.StudentId);
                        if (existingStudent == null)
                        {
                            var student = new Student
                            {
                                UserId = studentUser.Id,
                                StudentId = studentData.StudentId,
                                EnrollmentDate = DateTime.UtcNow,
                                DateOfBirth = studentData.DateOfBirth,
                                Address = "123 Main St, City, State",
                                PhoneNumber = "+1234567893",
                                PhotoUrl = "/uploads/students/photos/default-student.jpg",
                                CreatedDate = DateTime.UtcNow
                            };

                            context.Students.Add(student);
                            Console.WriteLine($"Created student: {studentData.FirstName} {studentData.LastName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create student user {studentData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"Student user already exists: {studentData.Email}");
                }
            }

            await context.SaveChangesAsync();

            // Create sample enrollments only if they don't exist
            var students = await context.Students.ToListAsync();
            var classes = await context.Classes.Include(c => c.Course).ToListAsync();

            if (students.Any() && classes.Any() && !context.Enrollments.Any())
            {
                var enrollments = new List<Enrollment>();
                var random = new Random();

                foreach (var student in students)
                {
                    // Each student enrolled in 2-3 random classes
                    var classCount = random.Next(2, 4);
                    var selectedClasses = classes.OrderBy(x => random.Next()).Take(classCount).ToList();

                    foreach (var classEntity in selectedClasses)
                    {
                        if (classEntity.Course != null)
                        {
                            var existingEnrollment = await context.Enrollments
                                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.ClassId == classEntity.Id);

                            if (existingEnrollment == null)
                            {
                                enrollments.Add(new Enrollment
                                {
                                    StudentId = student.Id,
                                    CourseId = classEntity.Course.Id,
                                    ClassId = classEntity.Id,
                                    EnrollmentDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                                    Status = EnrollmentStatus.Active,
                                    CreatedDate = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                if (enrollments.Any())
                {
                    await context.Enrollments.AddRangeAsync(enrollments);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Created {enrollments.Count} enrollments");
                }
            }
            else
            {
                Console.WriteLine("Enrollments already exist or no students/classes available");
            }

            // Create sample assignments only if they don't exist
            var classesForAssignments = await context.Classes.ToListAsync();

            if (classesForAssignments.Any() && !context.Assignments.Any())
            {
                var assignments = new List<Assignment>();

                foreach (var classEntity in classesForAssignments)
                {
                    var assignmentTemplates = new[]
                    {
                        new { Title = "Midterm Project", Description = "Complete the midterm project as per requirements", DueDays = 14 },
                        new { Title = "Weekly Quiz 1", Description = "Multiple choice quiz covering week 1 topics", DueDays = 7 },
                        new { Title = "Research Paper", Description = "Write a 5-page research paper on assigned topic", DueDays = 21 }
                    };

                    foreach (var template in assignmentTemplates)
                    {
                        assignments.Add(new Assignment
                        {
                            Title = template.Title,
                            Description = template.Description,
                            DueDate = DateTime.UtcNow.AddDays(template.DueDays),
                            ClassId = classEntity.Id,
                            CreatedByTeacherId = classEntity.TeacherId,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }

                await context.Assignments.AddRangeAsync(assignments);
                await context.SaveChangesAsync();
                Console.WriteLine($"Created {assignments.Count} assignments");
            }
            else
            {
                Console.WriteLine("Assignments already exist or no classes available");
            }

            // Create sample attendance records only if they don't exist
            var activeEnrollments = await context.Enrollments
                .Include(e => e.Class)
                .Where(e => e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            if (activeEnrollments.Any() && !context.Attendances.Any())
            {
                var attendances = new List<Attendance>();
                var random = new Random();
                var startDate = DateTime.UtcNow.AddDays(-30);

                for (var date = startDate; date <= DateTime.UtcNow; date = date.AddDays(1))
                {
                    // Only add attendance for class days (Mon-Fri)
                    if (date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday)
                    {
                        foreach (var enrollment in activeEnrollments)
                        {
                            // Check if this is a class day based on DaysOfWeek
                            var dayChar = date.DayOfWeek.ToString().Substring(0, 1).ToUpper();
                            if (enrollment.Class.DaysOfWeek.Contains(dayChar))
                            {
                                var status = random.Next(100) switch
                                {
                                    < 80 => AttendanceStatus.Present,  // 80% present
                                    < 90 => AttendanceStatus.Absent,   // 10% absent
                                    < 95 => AttendanceStatus.Late,     // 5% late
                                    _ => AttendanceStatus.Excused      // 5% excused
                                };

                                attendances.Add(new Attendance
                                {
                                    ClassId = enrollment.ClassId,
                                    StudentId = enrollment.StudentId,
                                    Date = date.Date,
                                    Status = status,
                                    MarkedByTeacherId = (int)enrollment.Class.TeacherId,
                                    Remarks = status == AttendanceStatus.Excused ? "Medical leave" : null,
                                    CreatedDate = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                if (attendances.Any())
                {
                    await context.Attendances.AddRangeAsync(attendances);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Created {attendances.Count} attendance records");
                }
            }
            else
            {
                Console.WriteLine("Attendance records already exist or no active enrollments available");
            }

            Console.WriteLine("Database seeding completed successfully!");
        }
    }
}
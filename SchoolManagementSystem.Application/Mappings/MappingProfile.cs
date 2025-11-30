using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate));

            CreateMap<User, UserProfileDto>()
                .IncludeBase<User, UserDto>();

            CreateMap<UpdateUserDto, User>();

            // Role mappings
            CreateMap<IdentityRole, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.NormalizedName));

            // User Statistics
            CreateMap<UserStatsDto, UserStatsDto>();

            // Student mappings
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive));

            CreateMap<Student, StudentDetailDto>()
                .IncludeBase<Student, StudentDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedDate));

            CreateMap<CreateStudentDto, Student>()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateStudentDto, Student>()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<Student, StudentStatsDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));

            // Teacher mappings
            CreateMap<Teacher, TeacherDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive));

            CreateMap<Teacher, TeacherDetailDto>()
                .IncludeBase<Teacher, TeacherDto>();

            CreateMap<CreateTeacherDto, Teacher>()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateTeacherDto, Teacher>()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<Teacher, TeacherStatsDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));

            // Course mappings
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? $"{src.Teacher.User.FirstName} {src.Teacher.User.LastName}" : null));

            CreateMap<Course, CourseDetailDto>()
                .IncludeBase<Course, CourseDto>();

            CreateMap<CreateCourseDto, Course>();
            CreateMap<UpdateCourseDto, Course>();

            // Class mappings
            CreateMap<Class, ClassDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? $"{src.Teacher.User.FirstName} {src.Teacher.User.LastName}" : null));

            CreateMap<Class, ClassDetailDto>()
                .IncludeBase<Class, ClassDto>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.TeacherId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedDate));

            CreateMap<CreateClassDto, Class>();
            CreateMap<UpdateClassDto, Class>();

            // Assignment mappings
            // Assignment mappings with inheritance
            CreateMap<Assignment, AssignmentDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => $"{src.Class.Name} - {src.Class.Section}"))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Class.CourseId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Class.Course.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.CreatedByTeacher != null ?
                    $"{src.CreatedByTeacher.User.FirstName} {src.CreatedByTeacher.User.LastName}" : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedDate))
                .ForMember(dest => dest.TotalSubmissions, opt => opt.MapFrom(src => src.Submissions.Count))
                .ForMember(dest => dest.GradedSubmissions, opt => opt.MapFrom(src => src.Submissions.Count(s => !string.IsNullOrEmpty(s.Grade))));

            CreateMap<Assignment, AssignmentDetailDto>()
                .IncludeBase<Assignment, AssignmentDto>() 
                .ForMember(dest => dest.Submissions, opt => opt.MapFrom(src => src.Submissions));

            CreateMap<CreateAssignmentDto, Assignment>();
            CreateMap<UpdateAssignmentDto, Assignment>();

            // Submission mappings
            CreateMap<Submission, SubmissionDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.Student.User.FirstName} {src.Student.User.LastName}"))
                .ForMember(dest => dest.StudentIdNumber, opt => opt.MapFrom(src => src.Student.StudentId))
                .ForMember(dest => dest.AssignmentTitle, opt => opt.MapFrom(src => src.Assignment.Title))
                .ForMember(dest => dest.GradedByTeacherName, opt => opt.MapFrom(src =>
                    src.GradedByTeacher != null ?
                    $"{src.GradedByTeacher.User.FirstName} {src.GradedByTeacher.User.LastName}" : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedDate))
                .ForMember(dest => dest.TimeUntilDue, opt => opt.MapFrom(src =>
                    src.Assignment.DueDate - src.SubmittedDate));

            CreateMap<CreateSubmissionDto, Submission>()
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedDate, opt => opt.Ignore())
                .ForMember(dest => dest.StudentId, opt => opt.Ignore());

            CreateMap<UpdateSubmissionDto, Submission>()
                .ForMember(dest => dest.AssignmentId, opt => opt.Ignore())
                .ForMember(dest => dest.StudentId, opt => opt.Ignore())
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Grade, opt => opt.Ignore())
                .ForMember(dest => dest.GradedByTeacherId, opt => opt.Ignore());

            // Enrollment mappings
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.Student.User.FirstName} {src.Student.User.LastName}"))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => $"{src.Class.Name} - {src.Class.Section}"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateEnrollmentDto, Enrollment>();
            CreateMap<UpdateEnrollmentDto, Enrollment>();

            // Grade mappings
            CreateMap<Grade, GradeDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.Student.User.FirstName} {src.Student.User.LastName}"))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage))
                .ForMember(dest => dest.GradeLetter, opt => opt.MapFrom(src => src.GradeLetter));

            CreateMap<CreateGradeDto, Grade>();
            CreateMap<UpdateGradeDto, Grade>();

            CreateMap<BulkGradeItemDto, Grade>()
                .ForMember(dest => dest.AssessmentType, opt => opt.Ignore())
                .ForMember(dest => dest.AssessmentName, opt => opt.Ignore())
                .ForMember(dest => dest.TotalScore, opt => opt.Ignore())
                .ForMember(dest => dest.AssessmentDate, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore());

            // Attendance mappings
            CreateMap<Attendance, AttendanceDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.Student.User.FirstName} {src.Student.User.LastName}"))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => $"{src.Class.Name} - {src.Class.Section}"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateAttendanceDto, Attendance>();
            CreateMap<UpdateAttendanceDto, Attendance>();

            CreateMap<BulkAttendanceItemDto, Attendance>()
                .ForMember(dest => dest.ClassId, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.Ignore())
                .ForMember(dest => dest.MarkedByTeacherId, opt => opt.Ignore());

            CreateMap<Attendance, AttendanceSummaryDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.Student.User.FirstName} {src.Student.User.LastName}"))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Class.Course.Name))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Class.CourseId));

            // Department mappings
            CreateMap<Department, DepartmentDto>()
                .ForMember(dest => dest.HeadOfDepartmentName,
                    opt => opt.MapFrom(src =>
                        src.HeadOfDepartment != null ?
                        $"{src.HeadOfDepartment.User.FirstName} {src.HeadOfDepartment.User.LastName}" : null));

            CreateMap<Department, DepartmentDetailDto>()
                .IncludeBase<Department, DepartmentDto>();

            CreateMap<CreateDepartmentDto, Department>();
            CreateMap<UpdateDepartmentDto, Department>();

            // Notification mappings
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedDate))
                .ForMember(dest => dest.RecipientName, opt => opt.MapFrom(src =>
                    src.RecipientUser != null ?
                    $"{src.RecipientUser.FirstName} {src.RecipientUser.LastName}" : null));

            CreateMap<CreateNotificationDto, Notification>();

            // Report mappings
            CreateMap<Student, StudentReportDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.StudentIdNumber, opt => opt.MapFrom(src => src.StudentId));

            CreateMap<Teacher, TeacherReportDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId));

            CreateMap<Course, CourseReportDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? $"{src.Teacher.User.FirstName} {src.Teacher.User.LastName}" : null));

            CreateMap<Class, ClassReportDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => $"{src.Name} - {src.Section}"))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? $"{src.Teacher.User.FirstName} {src.Teacher.User.LastName}" : null));

            CreateMap<Course, CourseSummaryDto>()
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Enrollments.Count));

            CreateMap<Teacher, TeacherSummaryDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.Courses.Count))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src =>
                    src.Classes.Sum(c => c.Enrollments.Count)));

            // Dashboard mappings
            CreateMap<Student, StudentDashboardDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.StudentIdNumber, opt => opt.MapFrom(src => src.StudentId))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.User.UpdatedDate));

            CreateMap<Enrollment, CoursePerformanceDto>()
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course.Code))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => $"{src.Class.Name} - {src.Class.Section}"));

            // Bulk operation mappings
            CreateMap<BulkEnrollmentResultDto, BulkEnrollmentResultDto>();

            // Assignment stats (calculated in service, but map basic properties)
            CreateMap<Assignment, AssignmentStatsDto>()
                .ForMember(dest => dest.AssignmentTitle, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalStudents, opt => opt.Ignore())
                .ForMember(dest => dest.TotalSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.GradedSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.PendingSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.SubmissionRate, opt => opt.Ignore())
                .ForMember(dest => dest.AverageGrade, opt => opt.Ignore());

            // Submission stats (calculated in service)
            CreateMap<Assignment, SubmissionStatsDto>()
                .ForMember(dest => dest.AssignmentTitle, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalStudents, opt => opt.Ignore())
                .ForMember(dest => dest.TotalSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.GradedSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.PendingSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.SubmissionRate, opt => opt.Ignore())
                .ForMember(dest => dest.LateSubmissions, opt => opt.Ignore())
                .ForMember(dest => dest.OnTimeSubmissions, opt => opt.Ignore());

            // Financial report mappings
            CreateMap<Enrollment, RevenueItemDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => $"{src.Course.Name} Course Fees"))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Course.Fee));
        }
    }
}
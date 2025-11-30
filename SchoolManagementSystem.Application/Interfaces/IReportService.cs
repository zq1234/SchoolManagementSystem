using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IReportService
    {
        Task<StudentReportDto> GenerateStudentReportAsync(int studentId);
        Task<TeacherReportDto> GenerateTeacherReportAsync(int teacherId);
        Task<CourseReportDto> GenerateCourseReportAsync(int courseId);
        Task<ClassReportDto> GenerateClassReportAsync(int classId);
        Task<SchoolReportDto> GenerateSchoolReportAsync(DateTime startDate, DateTime endDate);
        Task<FinancialReportDto> GenerateFinancialReportAsync(int academicYear);

    }
}
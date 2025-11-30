**School Management System**
A comprehensive School Management System built with ASP.NET Core 9, featuring role-based access control, student/teacher management, attendance tracking, assignment submission, and grading system.

**Architecture**
This project follows Clean Architecture principles with the following structure:

SchoolManagementSystem/
├── src/
│ ├── SchoolManagementSystem.API/ # Web API Project (Controllers, Middleware)
│ ├── SchoolManagementSystem.Application/ # Application Layer (DTOs, Services, Interfaces)
│ ├── SchoolManagementSystem.Core/ # Domain Layer (Entities, Enums, Exceptions)
│ └── SchoolManagementSystem.Infrastructure/# Infrastructure Layer (DbContext, Repositories)
├── tests


**Database Structure**

Uses Entity Framework Core with code-first migrations and Identity tables.
Entity Relationship Diagram (ERD)

                                 ┌────────────────────────┐
                                 │         User           │
                                 │ Id (PK)                │
                                 │ FirstName              │
                                 │ LastName               │
                                 │ ... Identity fields    │
                                 └──────────┬─────────────┘
                                            │ 1–1
             ┌──────────────────────────────┼──────────────┐
             │                              │              │
             ▼                              ▼              ▼
┌────────────────────────┐      ┌────────────────────────┐      ┌────────────────────────┐
│        Student         │      │        Teacher         │      │      Notification      │
│ Id (PK)                │      │ Id (PK)                │      │ Id (PK)                │
│ StudentId (Unique)     │      │ EmployeeId (Unique)    │      │ RecipientUserId (FK)   │
│ UserId (FK → User)     │      │ UserId (FK → User)     │      └────────────────────────┘
└──────────┬─────────────┘      └──────────┬─────────────┘
           │                                │
           │ 1–M                           │ 1–M
           ▼                               ▼
┌────────────────────────┐      ┌──────────────────────────┐
│      Attendance        │      │      Assignment          │
│ Id (PK)                │      │ Id (PK)                  │
│ StudentId (FK)         │      │ ClassId (FK)             │
│ ClassId (FK)           │      │ CreatedByTeacherId (FK)  │
│ MarkedByTeacherId (FK) │      └──────────────────────────┘
└────────────────────────┘

                    ┌──────────────────────────┐
                    │       Department         │
                    │ Id (PK)                  │
                    │ HeadOfDepartmentId       │
                    │   (FK→Teacher/User)      │
                    └───────────┬──────────────┘
                                │ 1–M
                                ▼
                    ┌──────────────────────────┐
                    │         Course           │
                    │ Id (PK)                  │
                    │ Code (Unique)            │
                    │ TeacherId (FK → Teacher) │
                    │ DepartmentId (FK)        │
                    └───────────┬──────────────┘
                                │ 1–M
                                ▼
                    ┌──────────────────────────┐
                    │          Class           │
                    │ Id (PK)                  │
                    │ CourseId (FK)            │
                    │ TeacherId (FK)           │
                    └───────────┬──────────────┘
                                │ 1–M
                                ▼
                    ┌──────────────────────────┐
                    │        Enrollment        │
                    │ Id (PK)                  │
                    │ StudentId (FK)           │
                    │ CourseId (FK)            │
                    │ ClassId (FK)             │
                    └───────────┬──────────────┘
                                │ 1–M
                                ▼
                    ┌──────────────────────────┐
                    │          Grade           │
                    │ Id (PK)                  │
                    │ EnrollmentId (FK)        │
                    │ StudentId (FK)           │
                    │ CourseId (FK)            │
                    └──────────────────────────┘

Relationship Summary 
	1. User
		1–1 → Student
		1–1 → Teacher
		1–M → Notifications
		
	2. Teacher
		1–M → Courses (optional)
		1–M → Classes
		1–M → Assignments
		1–M → Attendances (as MarkedByTeacher)
		Can act as → Head of Department (FK in Department)
	
	3. Student
		1–M → Attendance
		1–M → Enrollment
		1–M → Grades
		1–1 → User
		
	4. Department
		1–M → Courses
		1 → HeadOfDepartment (User/Teacher)
	   
	5. Course
		1–M → Classes
		1–M → Enrollments
		1–M → Grades
		Many Courses can belong to one Teacher
		Many Courses can belong to one Department
		
	6. Class
		1–M → Assignments
		1–M → Attendance
		1–M → Enrollment
		Belongs to one Course
		Belongs to one Teacher
		
	7. Enrollment
		1–M → Grades
		Unique (StudentId + CourseId + ClassId)
		
	8. Grade
		Belongs to Student
		Belongs to Course
		Belongs to Enrollment
		
	9. Assignment
		Belongs to Class
		Belongs to Teacher
		
	10.Attendance
		Belongs to Student
		Belongs to Class
		Belongs to Teacher
		
	10.Notification
		Belongs to User
		
**## Features**

- Role-Based Access Control  (Admin, Teacher, Student)
- JWT Authentication & Authorization
- Student Management (Enrollment, Attendance, Grades)
- Teacher Management (Class assignment, Grading, Attendance)
- Course & Department Management
- Assignment Submission & Grading
- Real-time Notifications
- Caching & Performance Optimization
- Comprehensive Reporting
- Health Checks & Monitoring
- File Upload Support
- Automated Database Seeding

**## Technology Stack**

- Backend: ASP.NET Core 9.0
- Database: SQL Server
- ORM: Entity Framework Core 9.0
- Authentication: JWT Bearer Tokens
- Mapping: AutoMapper
- Logging: Serilog
- Validation: FluentValidation
- Caching: MemoryCache
- Documentation: Swagger/OpenAPI

  
## Prerequisites**

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

**## Quick Start**

**### 1. Clone the Repository**

git clone https://github.com/your-username/SchoolManagementSystem.git
cd SchoolManagementSystem

**### 2. Update Connection String**
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SchoolManagementDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
**### 3.Run Database Migrations**
dotnet ef migrations add InitialCreate --project src/SchoolManagementSystem.Infrastructure --startup-project src/SchoolManagementSystem.API
dotnet ef database update

**or**

From Visual Studio Package Manager Cosole 
SelectSchoolManagementSystem.Infrastructure as startup-project
run the following commands:
add-migration _Mig_1
update-database
Note: initially auto seeder will work after that it will skip seeding the data.

**### 4. Run the Application**

dotnet run

**API Documentation and Endpoints**

The API will be available at http://localhost:5166/index.html and Swagger documentation at https://localhost:5166/swagger. 
Swagger endpoints will automatically open as these are pre configured.
This provides:
Authentication
POST /api/auth/login - Login and receive JWT
All Admin APIs
All Teacher APIs
All Student APIs
Request/response samples
Role-based endpoint separation

**## Default Users**
The system seeds the database with default users:

**Super Admin**: 
  "email": "superadmin@sms.com",
  "password": "SuperAdmin123!"
__________________________________________
**Admin** :
  "email": "admin@sms.com",
  "password": "Admin123!"
__________________________________________
**Teacher** :
  "email": "john.smith@sms.com",
  "password": "Teacher123!"
__________________________________________
**Student**:
  "email": "bob.williams@sms.com",
  "password": "Student123!"

**## Configuration**
App Settings contains severla configurations which are as below
 1. JWT Token Configuration
    
  "Jwt": {
    "Secret": "your-super-secret-key",
    "Issuer": "SchoolManagementSystem",
    "Audience": "SchoolManagementSystemUsers",
    "ExpiryInMinutes": 60
  }

2. CacheSettings
   
"CacheSettings": {
  "DefaultExpirationMinutes": 10,
  "CourseListExpirationMinutes": 15,
  "UserListExpirationMinutes": 10,
  "StudentDashboardExpirationMinutes": 5,
  "SizeLimit": 100
}
**3. Logging Configuration**
  **3.1 Built-in Logging**
  
"LogLevel": {
  "Default": "Information",
  "Microsoft.AspNetCore": "Warning",
  "Microsoft.EntityFrameworkCore": "Warning",
  "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
  "System": "Warning"
}
  **3.2  Serilog Configuration**
  
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [
    { "Name": "Console" },
    { 
      "Name": "File",
      "Args": {
        "path": "logs/schoolmanagement-.txt",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 7
      }
    }

**4. CORS Configuration**

"Cors": {
  "AllowedOrigins": [ "http://localhost:3000", "https://localhost:3000" ]
}
**File Upload Settings**

"FileUploadSettings": {
  "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".pdf", ".docx" ],
  "MaxFileSizeMB": 10
}

**5. Email Configuration**

"Email": {
  "From": "noreply@sms.com",
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "Username": "schoolmanagement@gmail.com",
  "Password": "ycescvipavxinvpb"
}

**## Implementation Steps:**

1. Run Migrations & Seed Data
2. Update database with migrations
3. Run seed data to populate initial records
4. Test Authentication
5. Login with seeded users
   POST /api/auth/login
		{
		  "email": "admin@sms.com",
		  "password": "Admin123!"
		}
6. Test role-based access
7. Admin Operations
8. Create departments
    	POST /api/admin/departments
			{
			  "name": "Mechanical Engineering",
			  "description": "Mechanical Engg Dept",
			  "headOfDepartmentId": 3
			}
9. Create courses
10. Assign teachers to departments
11. Teacher Operations
12. Create classes
13. Mark attendance
14. Create assignments
15. Grade submissions
16. Student Operations
17. View enrolled classes
18. Submit assignments
19. View grades and attendance


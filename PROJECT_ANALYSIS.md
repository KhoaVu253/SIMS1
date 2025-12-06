# PHÂN TÍCH DỰ ÁN SIMS (Student Information Management System)

## 📋 Mục lục
1. [Tổng quan dự án](#tổng-quan-dự-án)
2. [Công nghệ và Framework](#công-nghệ-và-framework)
3. [Kiến trúc dự án](#kiến-trúc-dự-án)
4. [Cơ sở dữ liệu](#cơ-sở-dữ-liệu)
5. [Chức năng chính](#chức-năng-chính)
6. [Cấu trúc thư mục](#cấu-trúc-thư-mục)
7. [Các Model chính](#các-model-chính)
8. [Controllers](#controllers)
9. [Views](#views)
10. [Testing](#testing)
11. [Bảo mật](#bảo-mật)
12. [Tính năng nổi bật](#tính-năng-nổi-bật)

---

## 📌 Tổng quan dự án

**Tên dự án:** SIMS - Student Information Management System  
**Mô tả:** Hệ thống quản lý thông tin sinh viên, bao gồm quản lý sinh viên, giảng viên, môn học, lịch học, điểm số và phân công lớp học.

**Ngôn ngữ chính:** C# 12.0  
**Loại ứng dụng:** ASP.NET Core Razor Pages Web Application  
**Framework:** .NET 8.0  
**Kiến trúc:** MVC (Model-View-Controller)  
**Database:** Microsoft SQL Server (LocalDB)

---

## 🛠️ Công nghệ và Framework

### .NET Framework
- **.NET Version:** 8.0
- **C# Version:** 12.0
- **SDK:** Microsoft.NET.Sdk.Web
- **Nullable:** Enabled
- **Implicit Usings:** Enabled

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.3.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
```

### Frontend Technologies
- **HTML5 + Razor Syntax** (.cshtml)
- **CSS3** (Bootstrap 5)
- **JavaScript** (jQuery, Bootstrap JS)
- **Icons:** Bootstrap Icons

### Backend Technologies
- **ASP.NET Core MVC**
- **Entity Framework Core 8**
- **Cookie Authentication**
- **LINQ**

### Database
- **Database Engine:** SQL Server LocalDB
- **Connection String:** `Server=(localdb)\\MSSQLLocalDB;Database=SIMS;Trusted_Connection=True;MultipleActiveResultSets=true`
- **ORM:** Entity Framework Core với Code-First Migrations

---

## 🏗️ Kiến trúc dự án

### Pattern được sử dụng
1. **MVC (Model-View-Controller)**
   - **Models:** Định nghĩa cấu trúc dữ liệu và business logic
   - **Views:** Razor Pages để render UI
   - **Controllers:** Xử lý HTTP requests và business logic

2. **Repository Pattern (via DbContext)**
   - `ApplicationDbContext` đóng vai trò là Data Access Layer

3. **ViewModel Pattern**
   - Sử dụng ViewModels riêng cho từng màn hình
   - Tách biệt Domain Models và Presentation Models

4. **Service Layer**
   - `GradeCalculationService`: Xử lý logic tính điểm
   - Helper Classes: `ScheduleHelper`, `Extensions`, `Constants`

### Authentication & Authorization
- **Authentication Method:** Cookie-based Authentication
- **Roles:**
  - Admin
  - Faculty (Giảng viên)
  - Student (Sinh viên)
- **Authorization:** Role-based với `[Authorize(Roles = "...")]`

---

## 💾 Cơ sở dữ liệu

### Database Schema

#### Core Tables

**1. Users (Người dùng)**
```csharp
- Id: int (PK)
- Username: string (Unique)
- Password: string
- Role: string (Admin/Student/Faculty)
- IsActive: bool
```

**2. Students (Sinh viên)**
```csharp
- Id: int (PK)
- UserId: int (FK -> Users)
- StudentCode: string (Unique)
- FullName: string
- DateOfBirth: DateTime?
- Email: string
- Phone: string
- Department: string
- ClassName: string
- IsActive: bool
```

**3. Faculties (Giảng viên)**
```csharp
- Id: int (PK)
- UserId: int (FK -> Users)
- FacultyCode: string (Unique)
- FullName: string
- Email: string
- Phone: string
- Department: string
- IsActive: bool
```

**4. Courses (Môn học)**
```csharp
- Id: int (PK)
- CourseCode: string (Unique)
- CourseName: string
- Credits: int
- Department: string
- FacultyId: int? (FK -> Faculties) [Deprecated]
- IsActive: bool
```

**5. Enrollments (Đăng ký học)**
```csharp
- Id: int (PK)
- StudentId: int (FK -> Students)
- CourseId: int (FK -> Courses)
- ScheduleId: int? (FK -> CourseSchedules) [NEW]
- Semester: string (HK1/HK2/HK3)
- AcademicYear: string (2024-2025)
- EnrollmentDate: DateTime
- Status: string (Active/Completed/Dropped)
- MidtermScore: float?
- FinalScore: float?
- AverageScore: float?
- LetterGrade: string?
- AssignedByUserId: int? (FK -> Users)
- AssignedDate: DateTime?
- Notes: string?
```

**6. CourseSchedules (Lịch học)**
```csharp
- Id: int (PK)
- CourseId: int (FK -> Courses)
- FacultyId: int? (FK -> Faculties)
- Semester: string
- AcademicYear: string
- DayOfWeek: int (2-7: Thứ 2 - Thứ 7)
- StartPeriod: int (1-12)
- EndPeriod: int (1-12)
- Room: string
- Notes: string?
- IsActive: bool
```

**7. CourseFaculties (Quan hệ nhiều-nhiều giữa Course và Faculty)**
```csharp
- Id: int (PK)
- CourseId: int (FK -> Courses)
- FacultyId: int (FK -> Faculties)
- Role: string (Giảng viên chính/Giảng viên)
- ClassGroup: string? (Nhóm lớp)
- Notes: string?
- IsActive: bool
- AssignedDate: DateTime
```

### Relationships
- **User 1-1 Student:** One-to-One với Cascade Delete
- **User 1-1 Faculty:** One-to-One với Cascade Delete
- **Course 1-N Enrollments:** One-to-Many với Cascade Delete
- **Student 1-N Enrollments:** One-to-Many với Cascade Delete
- **CourseSchedule N-1 Course:** Many-to-One với Cascade Delete
- **CourseSchedule N-1 Faculty:** Many-to-One với Restrict Delete
- **Enrollment N-1 Schedule:** Many-to-One (Optional)
- **Course N-N Faculty:** Many-to-Many qua `CourseFaculties`

### Migrations
Dự án sử dụng **Entity Framework Core Migrations**:
- `20251204132837_InitialCreate.cs`: Tạo database ban đầu
- `20251204161505_UpdateEnrollmentWithGrades.cs`: Thêm điểm số vào Enrollment
- `20251205073346_AddCourseSchedule.cs`: Thêm bảng lịch học
- `20251205081534_AddCourseFacultyManyToMany.cs`: Thêm quan hệ nhiều-nhiều Course-Faculty
- `20251205083825_AddFacultyIdToCourseScheduleWithData.cs`: Thêm FacultyId vào Schedule

### Seeding Data
Dự án có seed data ban đầu:
- 1 Admin user: `admin / admin123`
- 1 Faculty user: `faculty001 / faculty123`
- 1 Student user: `SV001 / student123`
- 3 Sample courses (IT101, IT102, IT103)

---

## ⚙️ Chức năng chính

### 1. Quản lý tài khoản (Admin)
- ✅ Xem danh sách tất cả tài khoản
- ✅ Lọc theo Role (Admin/Student/Faculty)
- ✅ Tìm kiếm theo username
- ✅ Reset mật khẩu
- ✅ Khóa/mở khóa tài khoản
- ✅ Reset mật khẩu về mặc định (123456)

### 2. Quản lý sinh viên (Admin)
- ✅ Xem danh sách sinh viên
- ✅ Tìm kiếm sinh viên
- ✅ Thêm sinh viên mới
- ✅ Sửa thông tin sinh viên
- ✅ Xóa sinh viên (soft delete nếu có enrollment)
- ✅ Force delete (xóa hẳn cả enrollments)

### 3. Quản lý giảng viên (Admin)
- ✅ Xem danh sách giảng viên
- ✅ Tìm kiếm giảng viên
- ✅ Thêm giảng viên mới
- ✅ Sửa thông tin giảng viên
- ✅ Xóa giảng viên (soft delete nếu có môn dạy)
- ✅ Force delete

### 4. Quản lý môn học (Admin)
- ✅ Xem danh sách môn học
- ✅ Tìm kiếm môn học
- ✅ Thêm môn học mới
- ✅ Sửa thông tin môn học
- ✅ Xóa môn học (soft delete nếu có enrollment/schedule)
- ✅ Force delete (xóa hẳn cả related data)
- ✅ Phân công nhiều giảng viên cho một môn học
- ✅ Quản lý giảng viên của môn học

### 5. Quản lý lịch học (Admin)
- ✅ Xem lịch học theo học kỳ, năm học, thứ
- ✅ Tạo lịch học mới
- ✅ Phân công giảng viên cho lịch học
- ✅ Chọn phòng học, tiết học
- ✅ Kiểm tra xung đột lịch giảng viên
- ✅ Kiểm tra xung đột phòng học
- ✅ Sửa/xóa lịch học
- ✅ Xem số sinh viên đã đăng ký

### 6. Phân công sinh viên (Admin)
- ✅ Phân công sinh viên vào môn học
- ✅ Phân công sinh viên vào lớp học cụ thể (Schedule)
- ✅ Phân công theo lớp (bulk assign)
- ✅ Phân công cá nhân
- ✅ Lọc sinh viên theo khoa, lớp
- ✅ Xem danh sách enrollments
- ✅ Xóa enrollment

### 7. Chức năng sinh viên
- ✅ Dashboard: Xem tổng quan (số môn, tín chỉ, GPA)
- ✅ Xem danh sách môn đã đăng ký
- ✅ Xem lịch học theo tuần (weekly schedule)
- ✅ Xem điểm số theo học kỳ
- ✅ Lọc theo học kỳ, năm học

### 8. Chức năng giảng viên
- ✅ Dashboard: Xem tổng quan (số môn dạy, số sinh viên)
- ✅ Xem danh sách môn đang dạy
- ✅ Xem lịch dạy theo tuần
- ✅ Xem danh sách sinh viên từng môn
- ✅ Nhập điểm cho sinh viên (midterm, final)
- ✅ Tính điểm tự động (40% midterm + 60% final)
- ✅ Chuyển đổi điểm sang điểm chữ (A+, A, B+, ...)

### 9. Tính năng nâng cao
- ✅ **Data Isolation:** Giảng viên chỉ thấy sinh viên trong LỚP của mình
- ✅ **Schedule Conflict Detection:** Phát hiện xung đột lịch học
- ✅ **Auto Grade Calculation:** Tự động tính điểm trung bình và điểm chữ
- ✅ **Soft Delete:** Không xóa dữ liệu nếu có liên kết
- ✅ **Force Delete:** Cho phép admin xóa hoàn toàn
- ✅ **Role-based Authorization:** Phân quyền chặt chẽ

---

## 📁 Cấu trúc thư mục

```
SIMS/
├── Controllers/
│   ├── AccountController.cs      # Đăng nhập, đăng xuất
│   ├── AdminController.cs        # Quản lý toàn hệ thống
│   ├── FacultyController.cs      # Chức năng giảng viên
│   ├── StudentController.cs      # Chức năng sinh viên
│   └── HomeController.cs         # Trang chủ
│
├── Models/
│   ├── User.cs                   # Model người dùng
│   ├── Student.cs                # Model sinh viên
│   ├── Faculty.cs                # Model giảng viên
│   ├── Course.cs                 # Model môn học
│   ├── Enrollment.cs             # Model đăng ký học
│   ├── CourseSchedule.cs         # Model lịch học
│   ├── CourseFaculty.cs          # Model phân công giảng viên
│   ├── Grade.cs                  # Model điểm số
│   ├── ApplicationDbContext.cs   # DbContext
│   └── ViewModels/               # ViewModels cho Views
│       ├── LoginViewModel.cs
│       ├── StudentFormViewModel.cs
│       ├── FacultyFormViewModel.cs
│       ├── CourseFormViewModel.cs
│       ├── ScheduleViewModels.cs
│       ├── EnrollmentManagementViewModel.cs
│       ├── GradeEntryViewModel.cs
│       ├── CourseFacultyViewModels.cs
│       └── AccountManagementViewModel.cs
│
├── Views/
│   ├── Account/                  # Views đăng nhập
│   ├── Admin/                    # Views quản trị
│   │   ├── Index.cshtml
│   │   ├── Students.cshtml
│   │   ├── Faculties.cshtml
│   │   ├── Courses.cshtml
│   │   ├── ManageSchedules.cshtml
│   │   ├── ManageEnrollments.cshtml
│   │   ├── ManageAccounts.cshtml
│   │   └── ...
│   ├── Student/                  # Views sinh viên
│   │   ├── Dashboard.cshtml
│   │   ├── MySchedule.cshtml
│   │   ├── WeeklySchedule.cshtml
│   │   └── MyGrades.cshtml
│   ├── Faculty/                  # Views giảng viên
│   │   ├── Dashboard.cshtml
│   │   ├── MyCourses.cshtml
│   │   ├── MySchedule.cshtml
│   │   ├── EnrolledStudents.cshtml
│   │   └── EnterGrades.cshtml
│   └── Shared/
│       ├── _Layout.cshtml        # Layout chính
│       └── Error.cshtml
│
├── Helpers/
│   ├── Constants.cs              # Hằng số hệ thống
│   ├── Extensions.cs             # Extension methods
│   └── ScheduleHelper.cs         # Helper cho lịch học
│
├── Services/
│   └── GradeCalculationService.cs # Service tính điểm
│
├── Migrations/                   # EF Core Migrations
│   ├── 20251204132837_InitialCreate.cs
│   ├── 20251204161505_UpdateEnrollmentWithGrades.cs
│   ├── 20251205073346_AddCourseSchedule.cs
│   ├── 20251205081534_AddCourseFacultyManyToMany.cs
│   └── ...
│
├── wwwroot/                      # Static files
│   ├── css/
│   │   └── custom.css            # Custom CSS
│   ├── js/
│   └── lib/                      # Third-party libraries
│
├── appsettings.json              # Cấu hình ứng dụng
├── Program.cs                    # Entry point
└── SIMS.csproj                   # Project file
```

---

## 🎯 Các Model chính

### 1. User (Models/User.cs)
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // Admin, Student, Faculty
    public bool IsActive { get; set; }
    
    // Navigation
    public Student? Student { get; set; }
    public Faculty? Faculty { get; set; }
}
```

### 2. Student (Models/Student.cs)
```csharp
public class Student
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StudentCode { get; set; }
    public string FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Department { get; set; }
    public string ClassName { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public User User { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }
}
```

### 3. Course (Models/Course.cs)
```csharp
public class Course
{
    public int Id { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public int Credits { get; set; }
    public string Department { get; set; }
    public int? FacultyId { get; set; } // [Obsolete]
    public bool IsActive { get; set; }
    
    // Navigation
    public Faculty? Faculty { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }
    public ICollection<CourseFaculty> CourseFaculties { get; set; }
}
```

### 4. Enrollment (Models/Enrollment.cs)
```csharp
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? ScheduleId { get; set; } // NEW: Link to specific class
    public string Semester { get; set; }
    public string AcademicYear { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; }
    
    // Grades
    public float? MidtermScore { get; set; }
    public float? FinalScore { get; set; }
    public float? AverageScore { get; set; }
    public string? LetterGrade { get; set; }
    
    // Admin Assignment
    public int? AssignedByUserId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public Student Student { get; set; }
    public Course Course { get; set; }
    public CourseSchedule? Schedule { get; set; }
    public User? AssignedByUser { get; set; }
}
```

### 5. CourseSchedule (Models/CourseSchedule.cs)
```csharp
public class CourseSchedule
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int? FacultyId { get; set; }
    public string Semester { get; set; }
    public string AcademicYear { get; set; }
    public int DayOfWeek { get; set; } // 2-7
    public int StartPeriod { get; set; } // 1-12
    public int EndPeriod { get; set; }
    public string Room { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public Course Course { get; set; }
    public Faculty? Faculty { get; set; }
}
```

---

## 🎮 Controllers

### 1. AccountController
- **Login:** Đăng nhập với username/password
- **Logout:** Đăng xuất
- **AccessDenied:** Trang từ chối truy cập

### 2. AdminController
**Quản lý tài khoản:**
- `ManageAccounts()`: Danh sách tài khoản
- `ResetPassword()`: Reset mật khẩu
- `ToggleAccountStatus()`: Khóa/mở tài khoản

**Quản lý sinh viên:**
- `Students()`: Danh sách sinh viên
- `CreateStudent()`: Thêm sinh viên
- `EditStudent()`: Sửa sinh viên
- `DeleteStudent()`: Xóa sinh viên (soft)
- `ForceDeleteStudent()`: Xóa hẳn

**Quản lý giảng viên:**
- `Faculties()`: Danh sách giảng viên
- `CreateFaculty()`: Thêm giảng viên
- `EditFaculty()`: Sửa giảng viên
- `DeleteFaculty()`: Xóa giảng viên

**Quản lý môn học:**
- `Courses()`: Danh sách môn học
- `CreateCourse()`: Thêm môn học
- `EditCourse()`: Sửa môn học
- `DeleteCourse()`: Xóa môn học
- `ManageCourseFaculties()`: Quản lý giảng viên của môn
- `AssignFacultyToCourse()`: Phân công giảng viên

**Quản lý lịch học:**
- `ManageSchedules()`: Danh sách lịch học
- `CreateSchedule()`: Tạo lịch học
- `EditSchedule()`: Sửa lịch học
- `DeleteSchedule()`: Xóa lịch học
- `GetCourseFaculties()`: API lấy giảng viên của môn
- `GetCourseSchedules()`: API lấy lịch của môn

**Phân công sinh viên:**
- `ManageEnrollments()`: Danh sách phân công
- `AssignStudents()`: Phân công cá nhân
- `BulkAssign()`: Phân công hàng loạt
- `RemoveEnrollment()`: Xóa phân công
- `GetStudentsByDepartment()`: API lấy sinh viên theo khoa

### 3. FacultyController
- `Dashboard()`: Trang chủ giảng viên
- `MyCourses()`: Môn đang dạy
- `MySchedule()`: Lịch dạy theo tuần
- `EnrolledStudents()`: Sinh viên từng môn
- `EnterGrades()`: Nhập điểm

### 4. StudentController
- `Dashboard()`: Trang chủ sinh viên
- `MySchedule()`: Môn đã đăng ký
- `WeeklySchedule()`: Lịch học theo tuần
- `MyGrades()`: Xem điểm

---

## 🎨 Views

### Layout chính (_Layout.cshtml)
- Bootstrap 5 responsive
- Navigation bar với role-based menu
- Footer
- Toast notifications (TempData)

### Admin Views
- Dashboard với statistics cards
- CRUD forms cho Students, Faculties, Courses
- Schedule management với time conflict detection
- Enrollment management với filtering

### Student Views
- Dashboard với progress cards
- Weekly schedule table (7 days x 12 periods)
- Grade list với color coding
- Responsive design

### Faculty Views
- Dashboard với teaching statistics
- Weekly teaching schedule
- Student list per course
- Grade entry form với auto-calculation

---

## 🧪 Testing

**Hiện trạng:**
- ❌ **Không có Testing Framework** được cấu hình
- ❌ Không có Unit Tests
- ❌ Không có Integration Tests
- ❌ Không có Test Project

**Đề xuất:**
- Nên thêm **xUnit** hoặc **NUnit** cho Unit Testing
- Nên thêm **Integration Tests** cho Controllers
- Nên thêm **In-Memory Database** cho testing
- Nên mock `DbContext` để test logic

**Cách thêm Testing (đề xuất):**
```bash
dotnet new xunit -n SIMS.Tests
dotnet add SIMS.Tests reference SIMS/SIMS.csproj
dotnet add SIMS.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add SIMS.Tests package Moq
```

---

## 🔒 Bảo mật

### Authentication
- **Cookie-based Authentication** với ExpireTimeSpan = 8 giờ
- **Sliding Expiration:** enabled
- **Password:** Lưu plain text (⚠️ **KHÔNG AN TOÀN**)

### Authorization
- **Role-based Authorization:**
  - `[Authorize(Roles = "Admin")]`
  - `[Authorize(Roles = "Faculty")]`
  - `[Authorize(Roles = "Student")]`

### Vấn đề bảo mật
⚠️ **CÁC VẤN ĐỀ CẦN FIX:**
1. **Password không được hash** - Lưu plain text trong database
2. **Không có password validation** - Không yêu cầu độ phức tạp
3. **Không có rate limiting** - Có thể bị brute force
4. **Không có HTTPS enforcement** (chỉ trong development)
5. **Không có CSRF token** cho một số forms
6. **Không có input sanitization** - Có thể bị SQL injection (mặc dù EF Core giảm thiểu)

**Đề xuất cải thiện:**
```csharp
// Sử dụng BCrypt hoặc ASP.NET Core Identity
using BCrypt.Net;
user.Password = BCrypt.HashPassword(password);
bool isValid = BCrypt.Verify(password, user.Password);
```

---

## ✨ Tính năng nổi bật

### 1. Data Isolation (Cô lập dữ liệu)
- Giảng viên **CHỈ** thấy sinh viên trong lớp mình dạy
- Sinh viên **CHỈ** thấy điểm và lịch của mình
- Admin thấy tất cả dữ liệu

### 2. Schedule Conflict Detection
```csharp
// Kiểm tra xung đột lịch giảng viên
if (ScheduleHelper.IsTimeConflict(...)) {
    ModelState.AddModelError("", "Giảng viên đã có lịch dạy!");
}

// Kiểm tra xung đột phòng học
if (roomConflicts.Any()) {
    ModelState.AddModelError("Room", "Phòng đã được sử dụng!");
}
```

### 3. Auto Grade Calculation
```csharp
// 40% midterm + 60% final
enrollment.AverageScore = (midterm * 0.4f) + (final * 0.6f);

// Convert to letter grade
enrollment.LetterGrade = averageScore switch {
    >= 9.0f => "A+",
    >= 8.5f => "A",
    >= 8.0f => "B+",
    // ...
    _ => "F"
};
```

### 4. Soft Delete
```csharp
// Không xóa nếu có dữ liệu liên quan
if (student.Enrollments.Any()) {
    student.IsActive = false; // Soft delete
} else {
    _context.Students.Remove(student); // Hard delete
}
```

### 5. Multiple Faculty Assignment
- Một môn học có thể có **nhiều giảng viên**
- Phân biệt "Giảng viên chính" và "Giảng viên"
- Hỗ trợ nhiều nhóm lớp (ClassGroup)

### 6. Flexible Student Assignment
- Phân công vào **môn học** (không gắn lớp cụ thể)
- Phân công vào **lớp học cụ thể** (ScheduleId)
- Bulk assign theo khoa, lớp

---

## 📊 Statistics

### Code Metrics
- **Controllers:** 4 files (~3,500 lines)
- **Models:** 10+ files (~1,000 lines)
- **Views:** 30+ files (~5,000 lines)
- **Migrations:** 5 files
- **Helpers:** 3 files

### Database Tables
- **Core Tables:** 7 (Users, Students, Faculties, Courses, Enrollments, CourseSchedules, CourseFaculties)
- **Total Columns:** ~80+ columns
- **Relationships:** 10+ foreign keys

### Features Count
- **Admin Features:** 40+ actions
- **Faculty Features:** 8 actions
- **Student Features:** 5 actions
- **Total Endpoints:** 50+ actions

---

## 📝 Documentation Files

Dự án có các file documentation:
1. `BUGFIX_COURSECODE_NULLREFERENCE.md`
2. `BUGFIX_DELETE_COURSE.md`
3. `BUGFIX_MISSING_ROOM_FIELD_CREATE_SCHEDULE.md`
4. `BUGFIX_MODAL_DELETE_SCHEDULE.md`
5. `BUGFIX_SCHEDULE_CREATION_EMPTY_FACULTY_DROPDOWN.md`
6. `FEATURE_ASSIGN_STUDENTS_TO_SPECIFIC_CLASS.md`
7. `FEATURE_FACULTY_IN_SCHEDULE.md`
8. `FEATURE_MULTIPLE_FACULTIES_PER_COURSE.md`
9. `FIX_DISPLAY_SCHEDULE_FACULTY.md`
10. `FIX_FACULTY_DATA_ISOLATION.md`
11. `UPDATE_FACULTY_SCHEDULE_ENROLLMENT_FILTER.md`

---

## 🚀 Deployment

### Development
```bash
# Restore packages
dotnet restore

# Run migrations
dotnet ef database update

# Run application
dotnet run
```

### Production (đề xuất)
1. Đổi connection string sang SQL Server thực
2. Enable HTTPS
3. Hash passwords
4. Thêm logging (Serilog)
5. Thêm Application Insights
6. Setup CI/CD với GitHub Actions hoặc Azure DevOps

---

## 🔧 Cải thiện đề xuất

### 1. Bảo mật
- [ ] Hash passwords (BCrypt hoặc Identity)
- [ ] HTTPS enforcement
- [ ] Rate limiting
- [ ] Input validation và sanitization
- [ ] CSRF tokens cho tất cả forms

### 2. Testing
- [ ] Thêm Unit Tests
- [ ] Thêm Integration Tests
- [ ] Thêm E2E Tests (Selenium)
- [ ] Code coverage > 80%

### 3. Performance
- [ ] Add caching (Redis)
- [ ] Optimize database queries
- [ ] Add indexes
- [ ] Implement pagination
- [ ] Lazy loading cho navigation properties

### 4. Code Quality
- [ ] Add code comments
- [ ] Add XML documentation
- [ ] Follow SOLID principles
- [ ] Refactor long methods
- [ ] Add error handling middleware

### 5. Features
- [ ] Email notifications
- [ ] File upload (syllabus, assignments)
- [ ] Attendance tracking
- [ ] Report generation (PDF/Excel)
- [ ] Mobile app (Xamarin/MAUI)

---

## 📧 Contact & Support

**Repository:** https://github.com/KhoaVu253/SIMS1  
**Branch:** main  
**Last Updated:** December 2024

---

## 📜 License

(Chưa có thông tin license trong project)

---

**Tổng kết:**  
Đây là một dự án ASP.NET Core MVC hoàn chỉnh với kiến trúc tốt, sử dụng Entity Framework Core, hỗ trợ role-based authentication, và có nhiều tính năng quản lý phức tạp. Dự án có potential để phát triển thành hệ thống SIS production-ready nếu cải thiện về bảo mật, testing, và performance.

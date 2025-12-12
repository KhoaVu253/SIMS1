using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.Models;
using SIMS.Models.ViewModels;
using SIMS.Helpers;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalStudents = _context.Students.Count(s => s.IsActive);
            ViewBag.TotalFaculties = _context.Faculties.Count(f => f.IsActive);
            ViewBag.TotalCourses = _context.Courses.Count(c => c.IsActive);
            ViewBag.TotalEnrollments = _context.Enrollments.Count();
            ViewBag.TotalUsers = _context.Users.Count(u => u.IsActive);
            ViewBag.InactiveUsers = _context.Users.Count(u => !u.IsActive);
            return View();
        }

        // ============================================
        // ACCOUNT MANAGEMENT
        // ============================================

        [HttpGet]
        public async Task<IActionResult> ManageAccounts(string role, string searchString)
        {
            var query = _context.Users
                .Include(u => u.Student)
                .Include(u => u.Faculty)
                .AsQueryable();

            // Filter by role
            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                query = query.Where(u => u.Role == role);
            }

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Username.Contains(searchString));
            }

            var users = await query
                .Select(u => new AccountManagementViewModel
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Role = u.Role,
                    FullName = u.Role == "Student" 
                        ? (u.Student != null ? u.Student.FullName : "")
                        : u.Role == "Faculty" 
                            ? (u.Faculty != null ? u.Faculty.FullName : "")
                            : "Admin",
                    Email = u.Role == "Student"
                        ? (u.Student != null ? u.Student.Email : "")
                        : u.Role == "Faculty"
                            ? (u.Faculty != null ? u.Faculty.Email : "")
                            : "",
                    IsActive = u.IsActive
                })
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewBag.Role = role;
            ViewBag.SearchString = searchString;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            string fullName = "Admin";
            if (user.Role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
                fullName = student?.FullName ?? "";
            }
            else if (user.Role == "Faculty")
            {
                var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.UserId == id);
                fullName = faculty?.FullName ?? "";
            }

            var model = new ResetPasswordViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = fullName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.Password = model.NewPassword;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Password reset for account {user.Username}";
            return RedirectToAction(nameof(ManageAccounts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAccountStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;

            // Sync with Student/Faculty
            if (user.Role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
                if (student != null)
                {
                    student.IsActive = user.IsActive;
                }
            }
            else if (user.Role == "Faculty")
            {
                var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.UserId == id);
                if (faculty != null)
                {
                    faculty.IsActive = user.IsActive;
                }
            }

            await _context.SaveChangesAsync();

            var status = user.IsActive ? "activated" : "locked";
            TempData["Success"] = $"Account {user.Username} has been {status}";
            return RedirectToAction(nameof(ManageAccounts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordToDefault(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Password = "123456";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Password for {user.Username} has been reset to default (123456)";
            return RedirectToAction(nameof(ManageAccounts));
        }

        // ============================================
        // ENROLLMENT MANAGEMENT (Phân công sinh viên)
        // ============================================

        [HttpGet]
        public async Task<IActionResult> ManageEnrollments(string semester, string academicYear, int? courseId)
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.AssignedByUser)
                .AsQueryable();

            // Filter by semester
            if (!string.IsNullOrEmpty(semester))
            {
                query = query.Where(e => e.Semester == semester);
            }

            // Filter by academic year
            if (!string.IsNullOrEmpty(academicYear))
            {
                query = query.Where(e => e.AcademicYear == academicYear);
            }

            // Filter by course
            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
            }

            var enrollments = await query
                .Select(e => new EnrollmentListViewModel
                {
                    Id = e.Id,
                    StudentCode = e.Student.StudentCode,
                    StudentName = e.Student.FullName,
                    CourseCode = e.Course.CourseCode,
                    CourseName = e.Course.CourseName,
                    Semester = e.Semester,
                    AcademicYear = e.AcademicYear,
                    Status = e.Status,
                    AverageScore = e.AverageScore,
                    LetterGrade = e.LetterGrade,
                    AssignedDate = e.AssignedDate ?? e.EnrollmentDate,
                    AssignedBy = e.AssignedByUser != null ? e.AssignedByUser.Username : "System",
                    ScheduleId = e.ScheduleId // ✅ ADDED
                })
                .OrderByDescending(e => e.AssignedDate)
                .ToListAsync();

            ViewBag.Semester = semester;
            ViewBag.AcademicYear = academicYear;
            ViewBag.CourseId = courseId;
            ViewBag.Semesters = Constants.Semesters;
            ViewBag.Courses = new SelectList(
                await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                "Id", "CourseName");

            return View(enrollments);
        }

        [HttpGet]
        public async Task<IActionResult> AssignStudents()
        {
            ViewBag.Courses = new SelectList(
                await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                "Id", "CourseName");
            ViewBag.Departments = Constants.Departments;
            ViewBag.Semesters = Constants.Semesters;
            
            var model = new AssignStudentToCourseViewModel
            {
                Semester = Constants.CurrentSemester,
                AcademicYear = Constants.CurrentAcademicYear
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudents(AssignStudentToCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int assignedCount = 0;
                foreach (var studentId in model.StudentIds)
                {
                    // Check if already enrolled in this course for this semester/year
                    var exists = await _context.Enrollments.AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseId == model.CourseId &&
                        e.Semester == model.Semester &&
                        e.AcademicYear == model.AcademicYear);

                    if (!exists)
                    {
                        var enrollment = new Enrollment
                        {
                            StudentId = studentId,
                            CourseId = model.CourseId,
                            ScheduleId = model.ScheduleId, // ✅ NEW: Lớp cụ thể (nullable)
                            Semester = model.Semester,
                            AcademicYear = model.AcademicYear,
                            Status = "Active",
                            EnrollmentDate = DateTime.Now,
                            AssignedByUserId = currentUserId,
                            AssignedDate = DateTime.Now,
                            Notes = model.Notes
                        };

                        _context.Enrollments.Add(enrollment);
                        assignedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var scheduleInfo = model.ScheduleId.HasValue 
                    ? " to specific class" 
                    : " to course (no class assigned)";

                TempData["Success"] = $"{assignedCount} students have been assigned{scheduleInfo}!";

                return RedirectToAction(nameof(ManageEnrollments));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> BulkAssign()
        {
            ViewBag.Courses = new SelectList(
                await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                "Id", "CourseName");
            ViewBag.Departments = Constants.Departments;
            ViewBag.Semesters = Constants.Semesters;

            var model = new BulkAssignViewModel
            {
                Semester = Constants.CurrentSemester,
                AcademicYear = Constants.CurrentAcademicYear
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssign(BulkAssignViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            // ✅ Kiểm tra lớp học có tồn tại không
            var schedule = await _context.CourseSchedules
                .Include(s => s.Course)
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.Id == model.ScheduleId &&
                                         s.CourseId == model.CourseId &&
                                         s.Semester == model.Semester &&
                                         s.AcademicYear == model.AcademicYear);

            if (schedule == null)
            {
                ModelState.AddModelError("", "Class does not exist or does not match the selected course/semester");
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }

            // Get students based on filters
            var studentsQuery = _context.Students
                .Where(s => s.IsActive && s.Department == model.Department);

            if (!string.IsNullOrEmpty(model.ClassName))
            {
                studentsQuery = studentsQuery.Where(s => s.ClassName == model.ClassName);
            }

            var students = await studentsQuery.ToListAsync();

            if (!students.Any())
            {
                ModelState.AddModelError("", "No students found matching the criteria");
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int assignedCount = 0;
                foreach (var student in students)
                {
                    // ✅ Check if already enrolled in THIS SCHEDULE
                    var exists = await _context.Enrollments.AnyAsync(e =>
                        e.StudentId == student.Id &&
                        e.ScheduleId == model.ScheduleId &&
                        e.Semester == model.Semester &&
                        e.AcademicYear == model.AcademicYear);

                    if (!exists)
                    {
                        var enrollment = new Enrollment
                        {
                            StudentId = student.Id,
                            CourseId = model.CourseId,
                            ScheduleId = model.ScheduleId, // ✅ Gắn vào lớp cụ thể
                            Semester = model.Semester,
                            AcademicYear = model.AcademicYear,
                            Status = "Active",
                            EnrollmentDate = DateTime.Now,
                            AssignedByUserId = currentUserId,
                            AssignedDate = DateTime.Now,
                            Notes = model.Notes
                        };

                        _context.Enrollments.Add(enrollment);
                        assignedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var facultyName = schedule.Faculty != null ? schedule.Faculty.FullName : "Unknown";
                var timeInfo = $"{ScheduleHelper.GetDayName(schedule.DayOfWeek)}, {ScheduleHelper.GetTimeRange(schedule.StartPeriod, schedule.EndPeriod)}, Room {schedule.Room}";

                TempData["Success"] = $"{assignedCount}/{students.Count} students have been assigned to class!\n" +
                    $"Course: {schedule.Course.CourseName}\n" +
                    $"Faculty: {facultyName}\n" +
                    $"Schedule: {timeInfo}";

                return RedirectToAction(nameof(ManageEnrollments));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewClassDetails(int scheduleId, string semester, string academicYear)
        {
            // Lấy thông tin lớp học
            var schedule = await _context.CourseSchedules
                .Include(s => s.Course)
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
            {
                TempData["Error"] = "Class not found!";
                return RedirectToAction(nameof(ManageEnrollments));
            }

            // Lấy danh sách sinh viên trong lớp
            var students = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.ScheduleId == scheduleId &&
                           e.Semester == semester &&
                           e.AcademicYear == academicYear)
                .Select(e => new StudentInClassViewModel
                {
                    EnrollmentId = e.Id,
                    StudentCode = e.Student.StudentCode,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,
                    Phone = e.Student.Phone,
                    Status = e.Status,
                    MidtermScore = e.MidtermScore,
                    FinalScore = e.FinalScore,
                    AverageScore = e.AverageScore,
                    LetterGrade = e.LetterGrade,
                    EnrollmentDate = e.EnrollmentDate,
                    Notes = e.Notes
                })
                .OrderBy(s => s.StudentCode)
                .ToListAsync();

            var model = new ClassDetailsViewModel
            {
                ScheduleId = schedule.Id,
                CourseCode = schedule.Course.CourseCode,
                CourseName = schedule.Course.CourseName,
                Credits = schedule.Course.Credits,
                FacultyName = schedule.Faculty != null ? schedule.Faculty.FullName : "Chưa phân công",
                Semester = schedule.Semester,
                AcademicYear = schedule.AcademicYear,
                DayOfWeekName = ScheduleHelper.GetDayName(schedule.DayOfWeek),
                TimeRange = ScheduleHelper.GetTimeRange(schedule.StartPeriod, schedule.EndPeriod),
                Room = schedule.Room,
                Notes = schedule.Notes,
                Students = students
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsByDepartment(string department, string? className)
        {
            var query = _context.Students
                .Where(s => s.IsActive && s.Department == department);

            if (!string.IsNullOrEmpty(className))
            {
                query = query.Where(s => s.ClassName == className);
            }

            var students = await query
                .Select(s => new
                {
                    id = s.Id,
                    text = $"{s.StudentCode} - {s.FullName} - {s.ClassName}"
                })
                .ToListAsync();

            return Json(students);
        }

        // STUDENT MANAGEMENT
        public async Task<IActionResult> Students(string searchString)
        {
            var students = _context.Students.Include(s => s.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.StudentCode.Contains(searchString) 
                    || s.FullName.Contains(searchString)
                    || s.Department.Contains(searchString)
                    || s.ClassName.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
            return View(await students.OrderBy(s => s.StudentCode).ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateStudent()
        {
            ViewBag.Departments = Constants.Departments;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username == model.StudentCode))
            {
                ModelState.AddModelError("StudentCode", "Student code already exists");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create User
                var user = new User
                {
                    Username = model.StudentCode,
                    Password = model.Password ?? "123456",
                    Role = "Student",
                    IsActive = model.IsActive
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Student
                var student = new Student
                {
                    UserId = user.Id,
                    StudentCode = model.StudentCode,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Email = model.Email,
                    Phone = model.Phone,
                    Department = model.Department,
                    ClassName = model.ClassName,
                    IsActive = model.IsActive
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["Success"] = $"Student added successfully! Account: {user.Username} / Password: {user.Password}";
                return RedirectToAction(nameof(Students));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "An error occurred while adding student");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentFormViewModel
            {
                Id = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Email = student.Email,
                Phone = student.Phone,
                Department = student.Department,
                ClassName = student.ClassName,
                IsActive = student.IsActive
            };

            ViewBag.CurrentUsername = student.User.Username;
            ViewBag.Departments = Constants.Departments;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(StudentFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (student == null)
            {
                return NotFound();
            }

            student.FullName = model.FullName;
            student.DateOfBirth = model.DateOfBirth;
            student.Email = model.Email;
            student.Phone = model.Phone;
            student.Department = model.Department;
            student.ClassName = model.ClassName;
            student.IsActive = model.IsActive;

            if (!string.IsNullOrEmpty(model.Password))
            {
                student.User.Password = model.Password;
                TempData["Success"] = "Student updated and password changed successfully!";
            }
            else
            {
                TempData["Success"] = "Student updated successfully!";
            }

            student.User.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Kiểm tra có enrollment không
            if (student.Enrollments.Any())
            {
                // Có enrollment → chỉ khóa tài khoản (soft delete)
                student.IsActive = false;
                student.User.IsActive = false;
                await _context.SaveChangesAsync();
                
                TempData["Warning"] = $"Student has {student.Enrollments.Count} enrollments. Account has been locked instead of deleted.";
                return RedirectToAction(nameof(Students));
            }

            // Không có enrollment → xóa hẳn
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = student.UserId;
                
                // Xóa student trước (cascade sẽ tự xóa enrollments nếu có)
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // Xóa user
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Student deleted successfully!";
                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(Students));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.User)
                .Include(f => f.CourseFaculties) // ✅ FIX: Use CourseFaculties instead of Courses
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
            {
                return NotFound();
            }

            // Kiểm tra có course không
            if (faculty.CourseFaculties.Any())
            {
                // Có course → chỉ khóa tài khoản (soft delete)
                faculty.IsActive = false;
                faculty.User.IsActive = false;
                await _context.SaveChangesAsync();
                
                TempData["Warning"] = $"Faculty is teaching {faculty.CourseFaculties.Count} courses. Account has been locked instead of deleted.";
                return RedirectToAction(nameof(Faculties));
            }

            // Không có course → xóa hẳn
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = faculty.UserId;
                
                // Xóa faculty trước
                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();

                // Xóa user
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Faculty deleted successfully!";
                return RedirectToAction(nameof(Faculties));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(Faculties));
            }
        }

        // Thêm method xóa hẳn (force delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = student.UserId;
                var enrollmentCount = student.Enrollments.Count;

                // Xóa tất cả enrollments trước
                if (student.Enrollments.Any())
                {
                    _context.Enrollments.RemoveRange(student.Enrollments);
                    await _context.SaveChangesAsync();
                }

                // Xóa student
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // Xóa user
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = $"Student deleted successfully! ({enrollmentCount} enrollments deleted)";
                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(Students));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDeleteFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.User)
                .Include(f => f.CourseFaculties) // ✅ FIX: Use CourseFaculties instead of Courses
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = faculty.UserId;
                var courseCount = faculty.CourseFaculties.Count;

                // Remove CourseFaculties assignments
                if (faculty.CourseFaculties.Any())
                {
                    _context.CourseFaculties.RemoveRange(faculty.CourseFaculties);
                    await _context.SaveChangesAsync();
                }

                // Xóa faculty
                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();

                // Xóa user
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = $"Faculty deleted successfully! ({courseCount} course assignments deleted)";
                return RedirectToAction(nameof(Faculties));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(Faculties));
            }
        }

        // FACULTY MANAGEMENT
        public async Task<IActionResult> Faculties(string searchString)
        {
            var faculties = _context.Faculties.Include(f => f.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                faculties = faculties.Where(f => f.FacultyCode.Contains(searchString) 
                    || f.FullName.Contains(searchString)
                    || f.Department.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
            return View(await faculties.OrderBy(f => f.FacultyCode).ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateFaculty()
        {
            ViewBag.Departments = Constants.Departments;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFaculty(FacultyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Username == model.FacultyCode))
            {
                ModelState.AddModelError("FacultyCode", "Faculty code already exists");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Username = model.FacultyCode,
                    Password = model.Password ?? "123456",
                    Role = "Faculty",
                    IsActive = model.IsActive
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var faculty = new Faculty
                {
                    UserId = user.Id,
                    FacultyCode = model.FacultyCode,
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Department = model.Department,
                    IsActive = model.IsActive
                };
                _context.Faculties.Add(faculty);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["Success"] = $"Faculty added successfully! Account: {user.Username} / Password: {user.Password}";
                return RedirectToAction(nameof(Faculties));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "An error occurred while adding faculty");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
            {
                return NotFound();
            }

            var model = new FacultyFormViewModel
            {
                Id = faculty.Id,
                FacultyCode = faculty.FacultyCode,
                FullName = faculty.FullName,
                Email = faculty.Email,
                Phone = faculty.Phone,
                Department = faculty.Department,
                IsActive = faculty.IsActive
            };

            ViewBag.CurrentUsername = faculty.User.Username;
            ViewBag.Departments = Constants.Departments;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFaculty(FacultyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var faculty = await _context.Faculties
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == model.Id);

            if (faculty == null)
            {
                return NotFound();
            }

            faculty.FullName = model.FullName;
            faculty.Email = model.Email;
            faculty.Phone = model.Phone;
            faculty.Department = model.Department;
            faculty.IsActive = model.IsActive;

            if (!string.IsNullOrEmpty(model.Password))
            {
                faculty.User.Password = model.Password;
                TempData["Success"] = "Faculty updated and password changed successfully!";
            }
            else
            {
                TempData["Success"] = "Faculty updated successfully!";
            }

            faculty.User.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Faculties));
        }

        // COURSE MANAGEMENT
        public async Task<IActionResult> Courses(string searchString)
        {
            var courses = _context.Courses
                .Include(c => c.Faculty)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c => c.CourseCode.Contains(searchString) 
                    || c.CourseName.Contains(searchString)
                    || c.Department.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
            return View(await courses.OrderBy(c => c.CourseCode).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            ViewBag.Faculties = new SelectList(
                await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                "Id", "FullName");
            ViewBag.Departments = Constants.Departments;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseFormViewModel model, List<int> selectedFaculties)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                    "Id", "FullName");
                ViewBag.Departments = Constants.Departments;
                return View(model);
            }

            if (await _context.Courses.AnyAsync(c => c.CourseCode == model.CourseCode))
            {
                ModelState.AddModelError("CourseCode", "Course code already exists");
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                    "Id", "FullName");
                ViewBag.Departments = Constants.Departments;
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create course (giữ FacultyId null hoặc primary faculty)
                var course = new Course
                {
                    CourseCode = model.CourseCode,
                    CourseName = model.CourseName,
                    Credits = model.Credits,
                    Department = model.Department,
                    FacultyId = model.FacultyId, // Optional: Primary faculty
                    IsActive = model.IsActive
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // ✅ NEW: Assign multiple faculties
                if (selectedFaculties != null && selectedFaculties.Any())
                {
                    foreach (var facultyId in selectedFaculties)
                    {
                        var courseFaculty = new CourseFaculty
                        {
                            CourseId = course.Id,
                            FacultyId = facultyId,
                            Role = facultyId == model.FacultyId ? "Primary Faculty" : "Faculty",
                            IsActive = true,
                            AssignedDate = DateTime.Now
                        };
                        _context.CourseFaculties.Add(courseFaculty);
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Course added successfully!";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                    "Id", "FullName");
                ViewBag.Departments = Constants.Departments;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new CourseFormViewModel
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Credits = course.Credits,
                Department = course.Department,
                FacultyId = course.FacultyId,
                IsActive = course.IsActive
            };

            ViewBag.Faculties = new SelectList(
                await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                "Id", "FullName", course.FacultyId);
            ViewBag.Departments = Constants.Departments;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(CourseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                    "Id", "FullName");
                ViewBag.Departments = Constants.Departments; // ✅ THÊM DÒNG NÀY
                return View(model);
            }

            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null)
            {
                return NotFound();
            }

            course.CourseName = model.CourseName;
            course.Credits = model.Credits;
            course.Department = model.Department;
            course.FacultyId = model.FacultyId;
            course.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
                TempData["Success"] = "Course updated successfully!";
            return RedirectToAction(nameof(Courses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.CourseFaculties)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra có lịch học không
            var hasSchedules = await _context.CourseSchedules.AnyAsync(cs => cs.CourseId == id);

            // Kiểm tra có enrollment hoặc schedule không
            if (course.Enrollments.Any() || hasSchedules)
            {
                // Có data liên quan → chỉ khóa (soft delete)
                course.IsActive = false;
                await _context.SaveChangesAsync();
                
                var relatedCount = course.Enrollments.Count + (hasSchedules ? 1 : 0);
                TempData["Warning"] = $"Course has {course.Enrollments.Count} student enrollments" +
                    (hasSchedules ? " and schedules" : "") + 
                    ". Course has been deactivated instead of deleted.";
                return RedirectToAction(nameof(Courses));
            }

            // Không có data liên quan → xóa hẳn
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa CourseFaculties trước (nếu có)
                if (course.CourseFaculties.Any())
                {
                    _context.CourseFaculties.RemoveRange(course.CourseFaculties);
                    await _context.SaveChangesAsync();
                }

                // Xóa Course
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["Success"] = "Course deleted successfully!";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(Courses));
            }
        }

        // Thêm method xóa hẳn (force delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.CourseFaculties)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var enrollmentCount = course.Enrollments.Count;
                var facultyCount = course.CourseFaculties.Count;

                // Xóa tất cả lịch học
                var schedules = await _context.CourseSchedules.Where(cs => cs.CourseId == id).ToListAsync();
                if (schedules.Any())
                {
                    _context.CourseSchedules.RemoveRange(schedules);
                    await _context.SaveChangesAsync();
                }

                // Xóa tất cả enrollments
                if (course.Enrollments.Any())
                {
                    _context.Enrollments.RemoveRange(course.Enrollments);
                    await _context.SaveChangesAsync();
                }

                // Xóa tất cả CourseFaculties
                if (course.CourseFaculties.Any())
                {
                    _context.CourseFaculties.RemoveRange(course.CourseFaculties);
                    await _context.SaveChangesAsync();
                }

                // Xóa Course
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                TempData["Success"] = $"Course deleted successfully! " +
                    $"({enrollmentCount} enrollments, {schedules.Count} schedules, {facultyCount} faculty assignments deleted)";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(Courses));
            }
        }

        // ============================================
        // COURSE SCHEDULE MANAGEMENT
        // ============================================

        [HttpGet]
        public async Task<IActionResult> ManageSchedules(string semester, string academicYear, int? dayOfWeek)
        {
            var query = _context.CourseSchedules
                .Include(cs => cs.Course)
                .Include(cs => cs.Faculty) // ✅ Load giảng viên từ schedule, không phải course
                .AsQueryable();

            // Apply filters
            var currentSemester = semester ?? Constants.CurrentSemester;
            var currentYear = academicYear ?? Constants.CurrentAcademicYear;

            query = query.Where(cs => cs.Semester == currentSemester && cs.AcademicYear == currentYear);

            if (dayOfWeek.HasValue)
            {
                query = query.Where(cs => cs.DayOfWeek == dayOfWeek.Value);
            }

            var schedules = await query
                .Select(cs => new ManageScheduleViewModel
                {
                    ScheduleId = cs.Id,
                    CourseId = cs.CourseId,
                    CourseCode = cs.Course.CourseCode,
                    CourseName = cs.Course.CourseName,
                    FacultyName = cs.Faculty != null ? cs.Faculty.FullName : "Chưa phân công", // ✅ Lấy từ schedule
                    Semester = cs.Semester,
                    AcademicYear = cs.AcademicYear,
                    DayOfWeek = cs.DayOfWeek,
                    DayName = ScheduleHelper.GetDayName(cs.DayOfWeek),
                    StartPeriod = cs.StartPeriod,
                    EndPeriod = cs.EndPeriod,
                    PeriodRange = ScheduleHelper.GetPeriodRange(cs.StartPeriod, cs.EndPeriod),
                    TimeRange = ScheduleHelper.GetTimeRange(cs.StartPeriod, cs.EndPeriod),
                    Room = cs.Room,
                    IsActive = cs.IsActive,
                    EnrolledStudentsCount = _context.Enrollments.Count(e =>
                        e.CourseId == cs.CourseId &&
                        e.Semester == cs.Semester &&
                        e.AcademicYear == cs.AcademicYear &&
                        e.Status == "Active"),
                    Notes = cs.Notes
                })
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartPeriod)
                .ToListAsync();

            ViewBag.Semester = currentSemester;
            ViewBag.AcademicYear = currentYear;
            ViewBag.DayOfWeek = dayOfWeek;
            ViewBag.Semesters = Constants.Semesters;
            ViewBag.Days = ScheduleHelper.GetAllDays();

            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSchedule()
        {
            ViewBag.Courses = new SelectList(
                await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                "Id", "CourseName");
            ViewBag.Faculties = new SelectList(
                await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                "Id", "FullName");
            ViewBag.Semesters = Constants.Semesters;
            ViewBag.Days = ScheduleHelper.GetAllDays();
            ViewBag.Periods = ScheduleHelper.GetAllPeriods();

            var model = new CourseScheduleFormViewModel
            {
                Semester = Constants.CurrentSemester,
                AcademicYear = Constants.CurrentAcademicYear,
                IsActive = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(CourseScheduleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                    "Id", "FullName");
                ViewBag.Semesters = Constants.Semesters;
                ViewBag.Days = ScheduleHelper.GetAllDays();
                ViewBag.Periods = ScheduleHelper.GetAllPeriods();
                return View(model);
            }

            // Validate periods
            if (model.EndPeriod < model.StartPeriod)
            {
                ModelState.AddModelError("EndPeriod", "End period must be greater than or equal to start period");
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                    "Id", "FullName");
                ViewBag.Semesters = Constants.Semesters;
                ViewBag.Days = ScheduleHelper.GetAllDays();
                ViewBag.Periods = ScheduleHelper.GetAllPeriods();
                return View(model);
            }

            // Check faculty conflict
            var facultyConflicts = await _context.CourseSchedules
                .Include(cs => cs.Course)
                .Where(cs =>
                    cs.FacultyId == model.FacultyId &&
                    cs.Semester == model.Semester &&
                    cs.AcademicYear == model.AcademicYear &&
                    cs.DayOfWeek == model.DayOfWeek &&
                    cs.IsActive &&
                    cs.Id != model.Id)
                .ToListAsync();

            foreach (var conflict in facultyConflicts)
            {
                if (ScheduleHelper.IsTimeConflict(
                    conflict.DayOfWeek, conflict.StartPeriod, conflict.EndPeriod,
                    model.DayOfWeek, model.StartPeriod, model.EndPeriod))
                {
                    ModelState.AddModelError(
                        "", 
                        $"Faculty already has a class for '{conflict.Course.CourseName}' at {ScheduleHelper.GetTimeRange(conflict.StartPeriod, conflict.EndPeriod)}!");
                    ViewBag.Courses = new SelectList(
                        await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                        "Id", "CourseName");
                    ViewBag.Faculties = new SelectList(
                        await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                        "Id", "FullName");
                    ViewBag.Semesters = Constants.Semesters;
                    ViewBag.Days = ScheduleHelper.GetAllDays();
                    ViewBag.Periods = ScheduleHelper.GetAllPeriods();
                    return View(model);
                }
            }

            // Check room conflict
            var roomConflicts = await _context.CourseSchedules
                .Include(cs => cs.Course)
                .Where(cs =>
                    cs.Room == model.Room &&
                    cs.Semester == model.Semester &&
                    cs.AcademicYear == model.AcademicYear &&
                    cs.DayOfWeek == model.DayOfWeek &&
                    cs.IsActive)
                .ToListAsync();

            foreach (var conflict in roomConflicts)
            {
                if (ScheduleHelper.IsTimeConflict(
                    conflict.DayOfWeek, conflict.StartPeriod, conflict.EndPeriod,
                    model.DayOfWeek, model.StartPeriod, model.EndPeriod))
                {
                    ModelState.AddModelError(
                        "Room", 
                        $"Room {model.Room} is already used by '{conflict.Course.CourseName}' at {ScheduleHelper.GetTimeRange(conflict.StartPeriod, conflict.EndPeriod)}!");
                    ViewBag.Courses = new SelectList(
                        await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                        "Id", "CourseName");
                    ViewBag.Faculties = new SelectList(
                        await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                        "Id", "FullName");
                    ViewBag.Semesters = Constants.Semesters;
                    ViewBag.Days = ScheduleHelper.GetAllDays();
                    ViewBag.Periods = ScheduleHelper.GetAllPeriods();
                    return View(model);
                }
            }

            // Create schedule
            var schedule = new CourseSchedule
            {
                CourseId = model.CourseId,
                FacultyId = model.FacultyId,
                Semester = model.Semester,
                AcademicYear = model.AcademicYear,
                DayOfWeek = model.DayOfWeek,
                StartPeriod = model.StartPeriod,
                EndPeriod = model.EndPeriod,
                Room = model.Room,
                Notes = model.Notes,
                IsActive = model.IsActive
            };

            _context.CourseSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule created successfully!";
            return RedirectToAction(nameof(ManageSchedules));
        }

        [HttpGet]
        public async Task<IActionResult> EditSchedule(int id)
        {
            var schedule = await _context.CourseSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            var model = new CourseScheduleFormViewModel
            {
                Id = schedule.Id,
                CourseId = schedule.CourseId,
                FacultyId = schedule.FacultyId,
                Semester = schedule.Semester,
                AcademicYear = schedule.AcademicYear,
                DayOfWeek = schedule.DayOfWeek,
                StartPeriod = schedule.StartPeriod,
                EndPeriod = schedule.EndPeriod,
                Room = schedule.Room,
                Notes = schedule.Notes,
                IsActive = schedule.IsActive
            };

            ViewBag.Courses = new SelectList(
                await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                "Id", "CourseName", schedule.CourseId);
            ViewBag.Faculties = new SelectList(
                await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                "Id", "FullName", schedule.FacultyId);
            ViewBag.Semesters = Constants.Semesters;
            ViewBag.Days = ScheduleHelper.GetAllDays();
            ViewBag.Periods = ScheduleHelper.GetAllPeriods();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(CourseScheduleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(),
                    "Id", "FullName");
                ViewBag.Semesters = Constants.Semesters;
                ViewBag.Days = ScheduleHelper.GetAllDays();
                ViewBag.Periods = ScheduleHelper.GetAllPeriods();
                return View(model);
            }

            var schedule = await _context.CourseSchedules.FindAsync(model.Id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.CourseId = model.CourseId;
            schedule.FacultyId = model.FacultyId;
            schedule.Semester = model.Semester;
            schedule.AcademicYear = model.AcademicYear;
            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartPeriod = model.StartPeriod;
            schedule.EndPeriod = model.EndPeriod;
            schedule.Room = model.Room;
            schedule.Notes = model.Notes;
            schedule.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule updated successfully!";
            return RedirectToAction(nameof(ManageSchedules));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.CourseSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.CourseSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule deleted successfully!";
            return RedirectToAction(nameof(ManageSchedules));
        }

        /// <summary>
        /// API: Lấy danh sách giảng viên của một môn học
        /// Ưu tiên giảng viên đã được phân công, nhưng cũng cho phép chọn giảng viên khác
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCourseFaculties(int courseId)
        {
            // Lấy giảng viên đã được phân công cho môn này
            var assignedFaculties = await _context.CourseFaculties
                .Where(cf => cf.CourseId == courseId && cf.IsActive)
                .Include(cf => cf.Faculty)
                .Select(cf => new
                {
                    id = cf.FacultyId,
                    fullName = cf.Faculty.FullName,
                    role = cf.Role,
                    classGroup = cf.ClassGroup,
                    isAssigned = true
                })
                .ToListAsync();

            // Nếu có giảng viên đã được phân công, trả về danh sách đó
            if (assignedFaculties.Any())
            {
                return Json(assignedFaculties);
            }

            // Nếu chưa có, trả về TẤT CẢ giảng viên active để có thể chọn
            var allFaculties = await _context.Faculties
                .Where(f => f.IsActive)
                .Select(f => new
                {
                    id = f.Id,
                    fullName = f.FullName,
                    role = "Faculty",
                    classGroup = (string?)null,
                    isAssigned = false
                })
                .OrderBy(f => f.fullName)
                .ToListAsync();

            return Json(allFaculties);
        }

        /// <summary>
        /// ✅ NEW API: Lấy danh sách lớp/lịch học của một môn theo học kỳ và năm học
        /// Dùng cho dropdown chọn lớp khi phân công sinh viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCourseSchedules(int courseId, string semester, string academicYear)
        {
            var schedules = await _context.CourseSchedules
                .Include(cs => cs.Faculty)
                .Where(cs => cs.CourseId == courseId &&
                            cs.Semester == semester &&
                            cs.AcademicYear == academicYear &&
                            cs.IsActive)
                .Select(cs => new
                {
                    scheduleId = cs.Id,
                    facultyName = cs.Faculty != null ? cs.Faculty.FullName : "Not Assigned",
                    dayOfWeek = cs.DayOfWeek,
                    dayName = ScheduleHelper.GetDayName(cs.DayOfWeek),
                    startPeriod = cs.StartPeriod,
                    endPeriod = cs.EndPeriod,
                    periodRange = ScheduleHelper.GetPeriodRange(cs.StartPeriod, cs.EndPeriod),
                    timeRange = ScheduleHelper.GetTimeRange(cs.StartPeriod, cs.EndPeriod),
                    room = cs.Room
                })
                .OrderBy(cs => cs.dayOfWeek)
                .ThenBy(cs => cs.startPeriod)
                .ToListAsync();

            return Json(schedules);
        }

        // ============================================
        // COURSE-FACULTY MANAGEMENT (Multiple Faculties)
        // ============================================
        [HttpGet]
        public async Task<IActionResult> ManageCourseFaculties(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseFaculties)
                    .ThenInclude(cf => cf.Faculty)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            var viewModel = course.CourseFaculties
                .Select(cf => new CourseFacultyListViewModel
                {
                    CourseFacultyId = cf.Id,
                    CourseId = cf.CourseId,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    FacultyId = cf.FacultyId,
                    FacultyCode = cf.Faculty.FacultyCode,
                    FacultyName = cf.Faculty.FullName,
                    Role = cf.Role,
                    ClassGroup = cf.ClassGroup,
                    Notes = cf.Notes,
                    IsActive = cf.IsActive,
                    AssignedDate = cf.AssignedDate
                })
                .OrderBy(cf => cf.FacultyName)
                .ToList();

            ViewBag.CourseId = courseId;
            ViewBag.CourseCode = course.CourseCode;
            ViewBag.CourseName = course.CourseName;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AssignFacultyToCourse(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // Get faculties not yet assigned to this course
            var assignedFacultyIds = await _context.CourseFaculties
                .Where(cf => cf.CourseId == courseId && cf.IsActive)
                .Select(cf => cf.FacultyId)
                .ToListAsync();

            var availableFaculties = await _context.Faculties
                .Where(f => f.IsActive && !assignedFacultyIds.Contains(f.Id))
                .ToListAsync();

            ViewBag.Faculties = new SelectList(availableFaculties, "Id", "FullName");
            ViewBag.CourseId = courseId;
            ViewBag.CourseCode = course.CourseCode;
            ViewBag.CourseName = course.CourseName;

            var model = new AssignFacultyToCourseViewModel
            {
                CourseId = courseId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFacultyToCourse(AssignFacultyToCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                var assignedFacultyIds = await _context.CourseFaculties
                    .Where(cf => cf.CourseId == model.CourseId && cf.IsActive)
                    .Select(cf => cf.FacultyId)
                    .ToListAsync();

                var availableFaculties = await _context.Faculties
                    .Where(f => f.IsActive && !assignedFacultyIds.Contains(f.Id))
                    .ToListAsync();

                ViewBag.Faculties = new SelectList(availableFaculties, "Id", "FullName");
                ViewBag.CourseId = model.CourseId;
                ViewBag.CourseCode = course?.CourseCode;
                ViewBag.CourseName = course?.CourseName;

                return View(model);
            }

            try
            {
                foreach (var facultyId in model.FacultyIds)
                {
                    // Check if already assigned
                    var exists = await _context.CourseFaculties.AnyAsync(cf =>
                        cf.CourseId == model.CourseId &&
                        cf.FacultyId == facultyId &&
                        cf.IsActive);

                    if (!exists)
                    {
                        var courseFaculty = new CourseFaculty
                        {
                            CourseId = model.CourseId,
                            FacultyId = facultyId,
                            Role = model.Role,
                            ClassGroup = model.ClassGroup,
                            Notes = model.Notes,
                            IsActive = true,
                            AssignedDate = DateTime.Now
                        };

                        _context.CourseFaculties.Add(courseFaculty);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"{model.FacultyIds.Count} faculty members have been assigned to course!";
                return RedirectToAction(nameof(ManageCourseFaculties), new { courseId = model.CourseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");

                var course = await _context.Courses.FindAsync(model.CourseId);
                var assignedFacultyIds = await _context.CourseFaculties
                    .Where(cf => cf.CourseId == model.CourseId && cf.IsActive)
                    .Select(cf => cf.FacultyId)
                    .ToListAsync();

                var availableFaculties = await _context.Faculties
                    .Where(f => f.IsActive && !assignedFacultyIds.Contains(f.Id))
                    .ToListAsync();

                ViewBag.Faculties = new SelectList(availableFaculties, "Id", "FullName");
                ViewBag.CourseId = model.CourseId;
                ViewBag.CourseCode = course?.CourseCode;
                ViewBag.CourseName = course?.CourseName;

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCourseFaculty(int id, int courseId)
        {
            var courseFaculty = await _context.CourseFaculties.FindAsync(id);
            if (courseFaculty == null)
            {
                return NotFound();
            }

            _context.CourseFaculties.Remove(courseFaculty);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Faculty removed from course!";
            return RedirectToAction(nameof(ManageCourseFaculties), new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int id)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (enrollment == null)
                {
                    TempData["Error"] = "Enrollment not found!";
                    return RedirectToAction(nameof(ManageEnrollments));
                }

                // Kiểm tra xem sinh viên đã có điểm chưa
                if (enrollment.MidtermScore.HasValue || enrollment.FinalScore.HasValue || enrollment.AverageScore.HasValue)
                {
                    TempData["Warning"] = $"Cannot delete! Student {enrollment.Student.StudentCode} already has grades.";
                    return RedirectToAction(nameof(ManageEnrollments));
                }

                // Xóa enrollment
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Enrollment deleted for student {enrollment.Student.StudentCode} - {enrollment.Course.CourseName}";
                return RedirectToAction(nameof(ManageEnrollments));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting: {ex.Message}";
                return RedirectToAction(nameof(ManageEnrollments));
            }
        }
    }
}

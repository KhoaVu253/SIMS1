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

            TempData["Success"] = $"Đã reset mật khẩu cho tài khoản {user.Username}";
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

            var status = user.IsActive ? "kích hoạt" : "khóa";
            TempData["Success"] = $"Đã {status} tài khoản {user.Username}";
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

            TempData["Success"] = $"Đã reset mật khẩu của {user.Username} về mặc định (123456)";
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
                    AssignedBy = e.AssignedByUser != null ? e.AssignedByUser.Username : "System"
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
                foreach (var studentId in model.StudentIds)
                {
                    // Check if already enrolled
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
                            Semester = model.Semester,
                            AcademicYear = model.AcademicYear,
                            Status = "Active",
                            EnrollmentDate = DateTime.Now,
                            AssignedByUserId = currentUserId,
                            AssignedDate = DateTime.Now,
                            Notes = model.Notes
                        };

                        _context.Enrollments.Add(enrollment);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Đã phân công {model.StudentIds.Count} sinh viên vào môn học!";
                return RedirectToAction(nameof(ManageEnrollments));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                
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
                ModelState.AddModelError("", "Không tìm thấy sinh viên nào phù hợp với điều kiện");
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
                    // Check if already enrolled
                    var exists = await _context.Enrollments.AnyAsync(e =>
                        e.StudentId == student.Id &&
                        e.CourseId == model.CourseId &&
                        e.Semester == model.Semester &&
                        e.AcademicYear == model.AcademicYear);

                    if (!exists)
                    {
                        var enrollment = new Enrollment
                        {
                            StudentId = student.Id,
                            CourseId = model.CourseId,
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

                TempData["Success"] = $"Đã phân công {assignedCount}/{students.Count} sinh viên vào môn học!";
                return RedirectToAction(nameof(ManageEnrollments));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                
                ViewBag.Courses = new SelectList(
                    await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                    "Id", "CourseName");
                ViewBag.Departments = Constants.Departments;
                ViewBag.Semesters = Constants.Semesters;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa phân công thành công!";
            return RedirectToAction(nameof(ManageEnrollments));
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
                ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại");
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
                TempData["Success"] = $"Thêm sinh viên thành công! Tài khoản: {user.Username} / Mật khẩu: {user.Password}";
                return RedirectToAction(nameof(Students));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sinh viên");
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
                TempData["Success"] = "Cập nhật sinh viên và đổi mật khẩu thành công!";
            }
            else
            {
                TempData["Success"] = "Cập nhật sinh viên thành công!";
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
                
                TempData["Warning"] = $"Sinh viên đã có {student.Enrollments.Count} môn đăng ký. Tài khoản đã được khóa thay vì xóa.";
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
                TempData["Success"] = "Xóa sinh viên thành công!";
                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Lỗi khi xóa: {ex.Message}";
                return RedirectToAction(nameof(Students));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.User)
                .Include(f => f.Courses)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
            {
                return NotFound();
            }

            // Kiểm tra có course không
            if (faculty.Courses.Any())
            {
                // Có course → chỉ khóa tài khoản (soft delete)
                faculty.IsActive = false;
                faculty.User.IsActive = false;
                await _context.SaveChangesAsync();
                
                TempData["Warning"] = $"Giảng viên đang phụ trách {faculty.Courses.Count} môn học. Tài khoản đã được khóa thay vì xóa.";
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
                TempData["Success"] = "Xóa giảng viên thành công!";
                return RedirectToAction(nameof(Faculties));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Lỗi khi xóa: {ex.Message}";
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
                TempData["Success"] = $"Xóa sinh viên thành công! (Đã xóa {enrollmentCount} môn đăng ký)";
                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Lỗi khi xóa: {ex.Message}";
                return RedirectToAction(nameof(Students));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDeleteFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.User)
                .Include(f => f.Courses)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = faculty.UserId;
                var courseCount = faculty.Courses.Count;

                // Set all courses' FacultyId to null
                if (faculty.Courses.Any())
                {
                    foreach (var course in faculty.Courses)
                    {
                        course.FacultyId = null;
                    }
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
                TempData["Success"] = $"Xóa giảng viên thành công! ({courseCount} môn học không còn giảng viên)";
                return RedirectToAction(nameof(Faculties));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Lỗi khi xóa: {ex.Message}";
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
                ModelState.AddModelError("FacultyCode", "Mã giảng viên đã tồn tại");
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
                TempData["Success"] = $"Thêm giảng viên thành công! Tài khoản: {user.Username} / Mật khẩu: {user.Password}";
                return RedirectToAction(nameof(Faculties));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm giảng viên");
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
                TempData["Success"] = "Cập nhật giảng viên và đổi mật khẩu thành công!";
            }
            else
            {
                TempData["Success"] = "Cập nhật giảng viên thành công!";
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
        public async Task<IActionResult> CreateCourse(CourseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                    "Id", "FullName");
                return View(model);
            }

            if (await _context.Courses.AnyAsync(c => c.CourseCode == model.CourseCode))
            {
                ModelState.AddModelError("CourseCode", "Mã môn học đã tồn tại");
                ViewBag.Faculties = new SelectList(
                    await _context.Faculties.Where(f => f.IsActive).ToListAsync(), 
                    "Id", "FullName");
                return View(model);
            }

            var course = new Course
            {
                CourseCode = model.CourseCode,
                CourseName = model.CourseName,
                Credits = model.Credits,
                Department = model.Department,
                FacultyId = model.FacultyId,
                IsActive = model.IsActive
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm môn học thành công!";
            return RedirectToAction(nameof(Courses));
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
            TempData["Success"] = "Cập nhật môn học thành công!";
            return RedirectToAction(nameof(Courses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            course.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa môn học thành công!";
            return RedirectToAction(nameof(Courses));
        }
    }
}

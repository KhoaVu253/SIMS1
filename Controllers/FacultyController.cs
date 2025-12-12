using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SIMS.Models;
using SIMS.Models.ViewModels;
using SIMS.Helpers;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacultyController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Faculty?> GetCurrentFaculty()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _context.Faculties
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId);
        }

        // ✅ ADD: Index action redirect to Dashboard
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Dashboard()
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            // ✅ FIX: Chỉ đếm lịch dạy có sinh viên được phân công
            var scheduleIdsWithStudents = await _context.Enrollments
                .Where(e => e.ScheduleId != null &&
                           e.Semester == Constants.CurrentSemester &&
                           e.AcademicYear == Constants.CurrentAcademicYear &&
                           e.Status == "Active")
                .Select(e => e.ScheduleId!.Value)
                .Distinct()
                .ToListAsync();

            ViewBag.TotalCourses = await _context.CourseSchedules
                .Where(s => s.FacultyId == faculty.Id && 
                           s.IsActive &&
                           s.Semester == Constants.CurrentSemester &&
                           s.AcademicYear == Constants.CurrentAcademicYear &&
                           scheduleIdsWithStudents.Contains(s.Id)) // ✅ CHỈ đếm lịch có sinh viên
                .Select(s => s.CourseId)
                .Distinct()
                .CountAsync();

            // ✅ Đếm sinh viên được phân công vào LỚP của giảng viên
            ViewBag.TotalStudents = await _context.Enrollments
                .Include(e => e.Schedule)
                .Where(e => e.Schedule != null && 
                           e.Schedule.FacultyId == faculty.Id &&
                           e.Semester == Constants.CurrentSemester &&
                           e.AcademicYear == Constants.CurrentAcademicYear &&
                           e.Status == "Active") // ✅ Chỉ đếm Active
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            return View(faculty);
        }

        public async Task<IActionResult> MyCourses()
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            // ✅ FIX: Chỉ lấy lịch dạy có sinh viên được phân công
            var scheduleIdsWithStudents = await _context.Enrollments
                .Where(e => e.ScheduleId != null &&
                           e.Semester == Constants.CurrentSemester &&
                           e.AcademicYear == Constants.CurrentAcademicYear &&
                           e.Status == "Active")
                .Select(e => e.ScheduleId!.Value)
                .Distinct()
                .ToListAsync();

            // Lấy các môn học từ CourseSchedule mà giảng viên đang dạy VÀ có sinh viên
            var courseIds = await _context.CourseSchedules
                .Where(s => s.FacultyId == faculty.Id && 
                           s.IsActive &&
                           s.Semester == Constants.CurrentSemester &&
                           s.AcademicYear == Constants.CurrentAcademicYear &&
                           scheduleIdsWithStudents.Contains(s.Id)) // ✅ CHỈ lấy lịch có sinh viên
                .Select(s => s.CourseId)
                .Distinct()
                .ToListAsync();

            // Lấy courses với enrollments
            var coursesWithEnrollments = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Select(c => new
                {
                    Course = c,
                    Enrollments = _context.Enrollments
                        .Include(e => e.Student)
                        .Include(e => e.Schedule)
                        .Where(e => e.CourseId == c.Id &&
                                   e.Schedule != null &&
                                   e.Schedule.FacultyId == faculty.Id &&
                                   e.Semester == Constants.CurrentSemester &&
                                   e.AcademicYear == Constants.CurrentAcademicYear &&
                                   e.Status == "Active") // ✅ Chỉ lấy Active
                        .ToList()
                })
                .ToListAsync();

            // Map to Course objects
            var courses = coursesWithEnrollments.Select(x =>
            {
                x.Course.Enrollments = x.Enrollments;
                return x.Course;
            }).OrderBy(c => c.CourseCode).ToList();

            return View(courses);
        }

        // ✅ Lịch dạy của giảng viên
        [HttpGet]
        public async Task<IActionResult> MySchedule(string? semester, string? academicYear)
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            var currentSemester = semester ?? Constants.CurrentSemester;
            var currentYear = academicYear ?? Constants.CurrentAcademicYear;

            // ✅ FIX: Chỉ lấy lịch dạy có sinh viên đã được phân công
            var scheduleIdsWithStudents = await _context.Enrollments
                .Where(e => e.ScheduleId != null &&
                           e.Semester == currentSemester &&
                           e.AcademicYear == currentYear &&
                           e.Status == "Active")
                .Select(e => e.ScheduleId!.Value)
                .Distinct()
                .ToListAsync();

            // Lấy tất cả lịch dạy của giảng viên trong học kỳ
            var schedules = await _context.CourseSchedules
                .Include(s => s.Course)
                .Where(s => s.FacultyId == faculty.Id &&
                           s.Semester == currentSemester &&
                           s.AcademicYear == currentYear &&
                           s.IsActive &&
                           scheduleIdsWithStudents.Contains(s.Id)) // ✅ CHỈ lấy lịch có sinh viên
                .ToListAsync();

            // Build schedule items
            var scheduleItems = schedules.Select(s => new ScheduleItem
            {
                ScheduleId = s.Id,
                CourseId = s.CourseId,
                CourseCode = s.Course.CourseCode,
                CourseName = s.Course.CourseName,
                StartPeriod = s.StartPeriod,
                EndPeriod = s.EndPeriod,
                PeriodRange = ScheduleHelper.GetPeriodRange(s.StartPeriod, s.EndPeriod),
                TimeRange = ScheduleHelper.GetTimeRange(s.StartPeriod, s.EndPeriod),
                Room = s.Room,
                FacultyName = faculty.FullName,
                Credits = s.Course.Credits,
                SessionType = ScheduleHelper.GetSessionType(s.StartPeriod),
                ColorClass = ScheduleHelper.GetSessionColorClass(s.StartPeriod),
                DayOfWeek = s.DayOfWeek,
                Notes = s.Notes
            }).ToList();

            // Group by day of week
            var weekSchedule = ScheduleHelper.GetAllDays()
                .Select(day => new DaySchedule
                {
                    DayOfWeek = day.Value,
                    DayName = day.Name,
                    DayAbbr = ScheduleHelper.GetDayAbbreviation(day.Value),
                    Classes = scheduleItems
                        .Where(s => s.DayOfWeek == day.Value)
                        .OrderBy(s => s.StartPeriod)
                        .ToList()
                })
                .ToList();

            var model = new FacultyWeeklyScheduleViewModel
            {
                FacultyName = faculty.FullName,
                FacultyCode = faculty.FacultyCode,
                Semester = currentSemester,
                AcademicYear = currentYear,
                WeekSchedule = weekSchedule,
                TotalCourses = scheduleItems.Select(s => s.CourseId).Distinct().Count(),
                TotalClassesPerWeek = scheduleItems.Count
            };

            ViewBag.Semester = currentSemester;
            ViewBag.AcademicYear = currentYear;
            ViewBag.Semesters = Constants.Semesters;

            return View(model);
        }

        // ✅ FIX: Chỉ hiển thị sinh viên trong LỚP của giảng viên
        public async Task<IActionResult> EnrolledStudents(int courseId)
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            // Kiểm tra giảng viên có dạy môn này không
            var hasSchedule = await _context.CourseSchedules
                .AnyAsync(s => s.CourseId == courseId && 
                              s.FacultyId == faculty.Id &&
                              s.IsActive &&
                              s.Semester == Constants.CurrentSemester &&
                              s.AcademicYear == Constants.CurrentAcademicYear);

            if (!hasSchedule) return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            // ✅ FIX: Load enrollments riêng
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Schedule)
                .Where(e => e.CourseId == courseId &&
                           e.Schedule != null &&
                           e.Schedule.FacultyId == faculty.Id &&
                           e.Semester == Constants.CurrentSemester &&
                           e.AcademicYear == Constants.CurrentAcademicYear)
                .OrderBy(e => e.Student.StudentCode)
                .ToListAsync();

            course.Enrollments = enrollments;

            return View(course);
        }

        // ✅ FIX: Chỉ nhập điểm cho sinh viên trong LỚP của giảng viên
        [HttpGet]
        public async Task<IActionResult> EnterGrades(int courseId)
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            // Kiểm tra giảng viên có dạy môn này không
            var hasSchedule = await _context.CourseSchedules
                .AnyAsync(s => s.CourseId == courseId && 
                              s.FacultyId == faculty.Id &&
                              s.IsActive);

            if (!hasSchedule) return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            // ✅ FIX: Chỉ lấy sinh viên trong LỚP của giảng viên
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Schedule)
                .Where(e => e.CourseId == courseId &&
                           e.Schedule != null &&
                           e.Schedule.FacultyId == faculty.Id)
                .OrderBy(e => e.Student.StudentCode)
                .ToListAsync();

            var model = new GradeEntryViewModel
            {
                CourseId = course.Id,
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                Students = enrollments.Select(e => new StudentGradeEntry
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentCode = e.Student.StudentCode,
                    FullName = e.Student.FullName,
                    MidtermScore = e.MidtermScore,
                    FinalScore = e.FinalScore,
                    TotalScore = e.AverageScore,
                    LetterGrade = e.LetterGrade
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterGrades(GradeEntryViewModel model)
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            // Kiểm tra giảng viên có dạy môn này không
            var hasSchedule = await _context.CourseSchedules
                .AnyAsync(s => s.CourseId == model.CourseId && 
                              s.FacultyId == faculty.Id &&
                              s.IsActive);

            if (!hasSchedule) return NotFound();

            foreach (var studentGrade in model.Students)
            {
                // ✅ FIX: Kiểm tra enrollment thuộc lớp của giảng viên
                var enrollment = await _context.Enrollments
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.Id == studentGrade.EnrollmentId &&
                                             e.Schedule != null &&
                                             e.Schedule.FacultyId == faculty.Id);

                if (enrollment == null) continue;

                // Update scores in Enrollment
                enrollment.MidtermScore = studentGrade.MidtermScore;
                enrollment.FinalScore = studentGrade.FinalScore;

                // Calculate average score (40% midterm + 60% final)
                if (studentGrade.MidtermScore.HasValue && studentGrade.FinalScore.HasValue)
                {
                    enrollment.AverageScore = (studentGrade.MidtermScore.Value * 0.4f) + 
                                             (studentGrade.FinalScore.Value * 0.6f);
                }
                else
                {
                    enrollment.AverageScore = null;
                }

                // ✅ NEW: Calculate letter grade and check FAIL/PASS
                if (enrollment.AverageScore.HasValue)
                {
                    enrollment.LetterGrade = enrollment.AverageScore.Value switch
                    {
                        >= 9.0f => "A+",
                        >= 8.5f => "A",
                        >= 8.0f => "B+",
                        >= 7.0f => "B",
                        >= 6.5f => "C+",
                        >= 5.5f => "C",
                        >= 5.0f => "D+",
                        >= 4.0f => "D",
                        _ => "F"
                    };

                    // ✅ NEW: Đánh dấu TRƯỢT nếu < 5.0
                    if (enrollment.AverageScore.Value < 5.0f)
                    {
                        enrollment.Status = "Failed";
                        enrollment.IsFailed = true;
                        enrollment.LetterGrade = "F"; // Ensure F grade
                    }
                    else
                    {
                        enrollment.Status = "Completed";
                        enrollment.IsFailed = false;
                    }
                }
                else
                {
                    enrollment.LetterGrade = null;
                    // Keep current status if no score
                    if (enrollment.Status == "Active")
                    {
                        // Don't change Active status if no complete scores
                    }
                }
            }

            await _context.SaveChangesAsync();
            
            // ✅ Count failed students for feedback
            var failedCount = model.Students.Count(s => 
                s.MidtermScore.HasValue && 
                s.FinalScore.HasValue && 
                ((s.MidtermScore.Value * 0.4f) + (s.FinalScore.Value * 0.6f)) < 5.0f);
            
            if (failedCount > 0)
            {
                TempData["Warning"] = $"Grades saved successfully! {failedCount} students failed the course (< 5.0 points).";
            }
            else
            {
                TempData["Success"] = "Grades saved successfully! All students passed the course.";
            }
            
            return RedirectToAction(nameof(MyCourses));
        }
    }
}

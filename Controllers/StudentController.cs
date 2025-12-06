using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SIMS.Models;
using SIMS.Models.ViewModels;
using SIMS.Helpers;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Student?> GetCurrentStudent()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<IActionResult> Dashboard()
        {
            var student = await GetCurrentStudent();
            if (student == null) return NotFound();

            // Tính số môn đã được phân công trong học kỳ hiện tại
            ViewBag.TotalEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id &&
                           e.Semester == Constants.CurrentSemester &&
                           e.AcademicYear == Constants.CurrentAcademicYear)
                .CountAsync();

            // Tính tổng tín chỉ trong học kỳ hiện tại
            ViewBag.TotalCredits = await _context.Enrollments
                .Where(e => e.StudentId == student.Id &&
                           e.Semester == Constants.CurrentSemester &&
                           e.AcademicYear == Constants.CurrentAcademicYear)
                .Include(e => e.Course)
                .SumAsync(e => e.Course.Credits);

            // Tính điểm trung bình tích lũy (tất cả các học kỳ có điểm)
            var grades = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.AverageScore.HasValue)
                .Select(e => e.AverageScore!.Value)
                .ToListAsync();

            ViewBag.AverageGrade = grades.Any() ? grades.Average() : 0;

            return View(student);
        }

        // Index action - redirect to Dashboard
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult MyInfo()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // Xem lịch học (môn đã được phân công)
        public async Task<IActionResult> MySchedule(string? semester, string? academicYear)
        {
            var student = await GetCurrentStudent();
            if (student == null) return NotFound();

            var query = _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Faculty)
                .Where(e => e.StudentId == student.Id);

            // Filter by semester
            if (!string.IsNullOrEmpty(semester))
            {
                query = query.Where(e => e.Semester == semester);
            }
            else
            {
                // Default to current semester
                query = query.Where(e => e.Semester == Helpers.Constants.CurrentSemester &&
                                       e.AcademicYear == Helpers.Constants.CurrentAcademicYear);
            }

            if (!string.IsNullOrEmpty(academicYear))
            {
                query = query.Where(e => e.AcademicYear == academicYear);
            }

            var schedule = await query
                .Select(e => new StudentScheduleViewModel
                {
                    CourseCode = e.Course.CourseCode,
                    CourseName = e.Course.CourseName,
                    Credits = e.Course.Credits,
                    FacultyName = e.Course.Faculty != null ? e.Course.Faculty.FullName : "Chưa phân công",
                    Semester = e.Semester,
                    AcademicYear = e.AcademicYear,
                    Status = e.Status,
                    MidtermScore = e.MidtermScore,
                    FinalScore = e.FinalScore,
                    AverageScore = e.AverageScore,
                    LetterGrade = e.LetterGrade
                })
                .ToListAsync();

            ViewBag.Semester = semester ?? Helpers.Constants.CurrentSemester;
            ViewBag.AcademicYear = academicYear ?? Helpers.Constants.CurrentAcademicYear;
            ViewBag.Semesters = Helpers.Constants.Semesters;

            return View(schedule);
        }

        // MyGrades
        public async Task<IActionResult> MyGrades(string? semester)
        {
            var student = await GetCurrentStudent();
            if (student == null) return NotFound();

            var query = _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Faculty)
                .Where(e => e.StudentId == student.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(semester))
            {
                query = query.Where(e => e.Semester == semester);
            }

            var enrollments = await query
                .OrderBy(e => e.AcademicYear)
                .ThenBy(e => e.Semester)
                .ThenBy(e => e.Course.CourseCode)
                .ToListAsync();

            ViewBag.Semester = semester;
            ViewBag.Semesters = Constants.Semesters;

            return View(enrollments);
        }

        // ============================================
        // WEEKLY SCHEDULE - Lịch học theo tuần
        // ============================================

        [HttpGet]
        public async Task<IActionResult> WeeklySchedule(string? semester, string? academicYear)
        {
            var student = await GetCurrentStudent();
            if (student == null) return NotFound();

            var currentSemester = semester ?? Constants.CurrentSemester;
            var currentYear = academicYear ?? Constants.CurrentAcademicYear;

            // ✅ FIX: Lấy enrollments với ScheduleId (lớp đã được phân công)
            var enrollments = await _context.Enrollments
                .Include(e => e.Schedule)
                    .ThenInclude(s => s!.Faculty)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s!.Course)
                .Where(e => e.StudentId == student.Id &&
                           e.Semester == currentSemester &&
                           e.AcademicYear == currentYear &&
                           e.Status == "Active" &&
                           e.ScheduleId != null) // ✅ CHỈ lấy enrollment có ScheduleId
                .ToListAsync();

            if (!enrollments.Any())
            {
                // No schedules assigned
                var emptyModel = new StudentWeeklyScheduleViewModel
                {
                    StudentName = student.FullName,
                    StudentCode = student.StudentCode,
                    Semester = currentSemester,
                    AcademicYear = currentYear,
                    WeekSchedule = new List<DaySchedule>()
                };

                ViewBag.Semester = currentSemester;
                ViewBag.AcademicYear = currentYear;
                ViewBag.Semesters = Constants.Semesters;
                return View(emptyModel);
            }

            // ✅ Build schedule từ enrollments (CHỈ lớp đã phân công)
            var scheduleItems = enrollments
                .Where(e => e.Schedule != null)
                .Select(e => new ScheduleItem
                {
                    ScheduleId = e.Schedule!.Id,
                    CourseId = e.Schedule.CourseId,
                    CourseCode = e.Schedule.Course.CourseCode,
                    CourseName = e.Schedule.Course.CourseName,
                    StartPeriod = e.Schedule.StartPeriod,
                    EndPeriod = e.Schedule.EndPeriod,
                    PeriodRange = ScheduleHelper.GetPeriodRange(e.Schedule.StartPeriod, e.Schedule.EndPeriod),
                    TimeRange = ScheduleHelper.GetTimeRange(e.Schedule.StartPeriod, e.Schedule.EndPeriod),
                    Room = e.Schedule.Room,
                    FacultyName = e.Schedule.Faculty != null ? e.Schedule.Faculty.FullName : "Chưa phân công",
                    Credits = e.Schedule.Course.Credits,
                    SessionType = ScheduleHelper.GetSessionType(e.Schedule.StartPeriod),
                    ColorClass = ScheduleHelper.GetSessionColorClass(e.Schedule.StartPeriod),
                    DayOfWeek = e.Schedule.DayOfWeek,
                    Notes = e.Schedule.Notes
                })
                .ToList();

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

            var model = new StudentWeeklyScheduleViewModel
            {
                StudentName = student.FullName,
                StudentCode = student.StudentCode,
                Semester = currentSemester,
                AcademicYear = currentYear,
                WeekSchedule = weekSchedule,
                TotalCourses = enrollments.Select(e => e.CourseId).Distinct().Count(),
                TotalClassesPerWeek = scheduleItems.Count
            };

            ViewBag.Semester = currentSemester;
            ViewBag.AcademicYear = currentYear;
            ViewBag.Semesters = Constants.Semesters;

            return View(model);
        }
    }
}

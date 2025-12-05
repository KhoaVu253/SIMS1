using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SIMS.Models;
using SIMS.Models.ViewModels;

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

        public async Task<IActionResult> Dashboard()
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            ViewBag.TotalCourses = await _context.Courses
                .Where(c => c.FacultyId == faculty.Id && c.IsActive)
                .CountAsync();

            ViewBag.TotalStudents = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.Course.FacultyId == faculty.Id)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            return View(faculty);
        }

        public async Task<IActionResult> MyCourses()
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            var courses = await _context.Courses
                .Where(c => c.FacultyId == faculty.Id && c.IsActive)
                .Include(c => c.Enrollments)
                .OrderBy(c => c.CourseCode)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> EnrolledStudents(int courseId)
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.FacultyId == faculty.Id);

            if (course == null) return NotFound();

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> EnterGrades(int courseId)
        {
            var faculty = await GetCurrentFaculty();
            if (faculty == null) return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.FacultyId == faculty.Id);

            if (course == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId)
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

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == model.CourseId && c.FacultyId == faculty.Id);

            if (course == null) return NotFound();

            foreach (var studentGrade in model.Students)
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.Id == studentGrade.EnrollmentId);

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

                // Calculate letter grade
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

                    // Update status to Completed if has grade
                    if (enrollment.Status == "Active")
                    {
                        enrollment.Status = "Completed";
                    }
                }
                else
                {
                    enrollment.LetterGrade = null;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Lưu điểm thành công!";
            return RedirectToAction(nameof(MyCourses));
        }
    }
}

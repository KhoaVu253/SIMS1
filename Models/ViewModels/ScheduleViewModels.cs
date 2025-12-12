using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    // =============================================
    // ADMIN: Schedule Management
    // =============================================

    /// <summary>
    /// Form to create/edit schedule for Admin
    /// </summary>
    public class CourseScheduleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select a faculty")]
        [Display(Name = "Faculty")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [Display(Name = "Semester")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic year is required")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select day of week")]
        [Range(2, 8, ErrorMessage = "Day of week must be between 2 and 8")]
        [Display(Name = "Day of Week")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start period is required")]
        [Range(1, 12, ErrorMessage = "Period must be between 1 and 12")]
        [Display(Name = "Start Period")]
        public int StartPeriod { get; set; }

        [Required(ErrorMessage = "End period is required")]
        [Range(1, 12, ErrorMessage = "Period must be between 1 and 12")]
        [Display(Name = "End Period")]
        public int EndPeriod { get; set; }

        [Required(ErrorMessage = "Room is required")]
        [StringLength(50)]
        [Display(Name = "Room")]
        public string Room { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Schedule list for Admin
    /// </summary>
    public class ManageScheduleViewModel
    {
        public int ScheduleId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public string PeriodRange { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int EnrolledStudentsCount { get; set; }
        public string? Notes { get; set; }
    }

    // =============================================
    // STUDENT: View Schedule
    // =============================================

    /// <summary>
    /// Weekly schedule for student
    /// </summary>
    public class StudentWeeklyScheduleViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        /// <summary>
        /// Schedule from Monday to Sunday
        /// </summary>
        public List<DaySchedule> WeekSchedule { get; set; } = new();

        /// <summary>
        /// Total number of courses being taken
        /// </summary>
        public int TotalCourses { get; set; }

        /// <summary>
        /// Total number of classes per week
        /// </summary>
        public int TotalClassesPerWeek { get; set; }
    }

    /// <summary>
    /// Weekly teaching schedule for faculty
    /// </summary>
    public class FacultyWeeklyScheduleViewModel
    {
        public string FacultyName { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        /// <summary>
        /// Teaching schedule from Monday to Sunday
        /// </summary>
        public List<DaySchedule> WeekSchedule { get; set; } = new();

        /// <summary>
        /// Total number of courses being taught
        /// </summary>
        public int TotalCourses { get; set; }

        /// <summary>
        /// Total number of teaching sessions per week
        /// </summary>
        public int TotalClassesPerWeek { get; set; }
    }

    /// <summary>
    /// Schedule for one day
    /// </summary>
    public class DaySchedule
    {
        public int DayOfWeek { get; set; } // 2-8
        public string DayName { get; set; } = string.Empty; // "Monday"
        public string DayAbbr { get; set; } = string.Empty; // "Mon"
        public List<ScheduleItem> Classes { get; set; } = new();
        public bool HasClass => Classes.Any();
    }

    /// <summary>
    /// Details of one class session
    /// </summary>
    public class ScheduleItem
    {
        public int ScheduleId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; } // Day (2-8)
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public string PeriodRange { get; set; } = string.Empty; // "Period 1-3"
        public string TimeRange { get; set; } = string.Empty; // "07:00 - 09:30"
        public string Room { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string SessionType { get; set; } = string.Empty; // "Morning" / "Afternoon"
        public string ColorClass { get; set; } = string.Empty; // CSS class for color
        public string? Notes { get; set; }
    }

    // =============================================
    // CONFLICT DETECTION
    // =============================================

    /// <summary>
    /// Schedule conflict information
    /// </summary>
    public class ScheduleConflict
    {
        public string Type { get; set; } = string.Empty; // "Faculty", "Room", "Student"
        public string Message { get; set; } = string.Empty;
        public string ConflictWith { get; set; } = string.Empty;
        public bool IsWarning { get; set; } // true = warning, false = error
    }
}

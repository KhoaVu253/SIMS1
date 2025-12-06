using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    // =============================================
    // ADMIN: Quản lý lịch học
    // =============================================

    /// <summary>
    /// Form tạo/sửa lịch học cho Admin
    /// </summary>
    public class CourseScheduleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giảng viên")]
        [Display(Name = "Giảng viên")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        [Display(Name = "Năm học")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thứ")]
        [Range(2, 8, ErrorMessage = "Thứ phải từ 2 đến 8")]
        [Display(Name = "Thứ")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Tiết bắt đầu là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tiết từ 1 đến 12")]
        [Display(Name = "Tiết bắt đầu")]
        public int StartPeriod { get; set; }

        [Required(ErrorMessage = "Tiết kết thúc là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tiết từ 1 đến 12")]
        [Display(Name = "Tiết kết thúc")]
        public int EndPeriod { get; set; }

        [Required(ErrorMessage = "Phòng học là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Phòng học")]
        public string Room { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Danh sách lịch học cho Admin
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
    // STUDENT: Xem lịch học
    // =============================================

    /// <summary>
    /// Lịch học theo tuần của sinh viên
    /// </summary>
    public class StudentWeeklyScheduleViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        /// <summary>
        /// Lịch học từ Thứ 2 đến Chủ nhật
        /// </summary>
        public List<DaySchedule> WeekSchedule { get; set; } = new();

        /// <summary>
        /// Tổng số môn đang học
        /// </summary>
        public int TotalCourses { get; set; }

        /// <summary>
        /// Tổng số buổi học trong tuần
        /// </summary>
        public int TotalClassesPerWeek { get; set; }
    }

    /// <summary>
    /// ✅ NEW: Lịch dạy theo tuần của giảng viên
    /// </summary>
    public class FacultyWeeklyScheduleViewModel
    {
        public string FacultyName { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        /// <summary>
        /// Lịch dạy từ Thứ 2 đến Chủ nhật
        /// </summary>
        public List<DaySchedule> WeekSchedule { get; set; } = new();

        /// <summary>
        /// Tổng số môn đang dạy
        /// </summary>
        public int TotalCourses { get; set; }

        /// <summary>
        /// Tổng số buổi dạy trong tuần
        /// </summary>
        public int TotalClassesPerWeek { get; set; }
    }

    /// <summary>
    /// Lịch học trong một ngày
    /// </summary>
    public class DaySchedule
    {
        public int DayOfWeek { get; set; } // 2-8
        public string DayName { get; set; } = string.Empty; // "Thứ hai"
        public string DayAbbr { get; set; } = string.Empty; // "T2"
        public List<ScheduleItem> Classes { get; set; } = new();
        public bool HasClass => Classes.Any();
    }

    /// <summary>
    /// Chi tiết một buổi học
    /// </summary>
    public class ScheduleItem
    {
        public int ScheduleId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; } // ✅ ADDED: Thứ (2-8)
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public string PeriodRange { get; set; } = string.Empty; // "Tiết 1-3"
        public string TimeRange { get; set; } = string.Empty; // "07:00 - 09:30"
        public string Room { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string SessionType { get; set; } = string.Empty; // "Sáng" / "Chiều"
        public string ColorClass { get; set; } = string.Empty; // CSS class for color
        public string? Notes { get; set; }
    }

    // =============================================
    // CONFLICT DETECTION
    // =============================================

    /// <summary>
    /// Thông tin xung đột lịch
    /// </summary>
    public class ScheduleConflict
    {
        public string Type { get; set; } = string.Empty; // "Faculty", "Room", "Student"
        public string Message { get; set; } = string.Empty;
        public string ConflictWith { get; set; } = string.Empty;
        public bool IsWarning { get; set; } // true = warning, false = error
    }
}

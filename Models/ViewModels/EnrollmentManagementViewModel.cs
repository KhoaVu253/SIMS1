using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    // ViewModel cho việc phân công sinh viên vào lớp
    public class AssignStudentToCourseViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int CourseId { get; set; }

        /// <summary>
        /// ✅ NEW: Schedule ID - Lớp/lịch học cụ thể (optional)
        /// Nếu null = Phân công vào môn chung, không gắn lớp cụ thể
        /// Nếu có = Phân công vào lớp cụ thể với giảng viên, thời gian, phòng
        /// </summary>
        [Display(Name = "Lớp học")]
        public int? ScheduleId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 sinh viên")]
        [Display(Name = "Danh sách sinh viên")]
        public List<int> StudentIds { get; set; } = new();

        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        [Display(Name = "Năm học")]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // ViewModel cho việc xem danh sách phân công
    public class EnrollmentListViewModel
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public float? AverageScore { get; set; }
        public string? LetterGrade { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public int? ScheduleId { get; set; } // ✅ NEW: Lớp học cụ thể
    }

    // ViewModel cho việc phân công hàng loạt theo khoa
    public class BulkAssignViewModel
    {
        [Required]
        [Display(Name = "Môn học")]
        public int CourseId { get; set; }

        /// <summary>
        /// ✅ NEW: Lớp học cụ thể (bắt buộc)
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        [Display(Name = "Lớp học")]
        public int ScheduleId { get; set; }

        [Required]
        [Display(Name = "Khoa")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Lớp (để trống = tất cả)")]
        public string? ClassName { get; set; }

        [Required]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Năm học")]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }

    // ViewModel cho lịch học của sinh viên
    public class StudentScheduleViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public float? MidtermScore { get; set; }
        public float? FinalScore { get; set; }
        public float? AverageScore { get; set; }
        public string? LetterGrade { get; set; }
    }

    // ✅ NEW: ViewModel cho chi tiết lớp học
    public class ClassDetailsViewModel
    {
        // Thông tin lớp học
        public int ScheduleId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string DayOfWeekName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Danh sách sinh viên trong lớp
        public List<StudentInClassViewModel> Students { get; set; } = new();

        // Thống kê
        public int TotalStudents => Students.Count;
        public int ActiveStudents => Students.Count(s => s.Status == "Active");
        public int CompletedStudents => Students.Count(s => s.Status == "Completed");
        public int DroppedStudents => Students.Count(s => s.Status == "Dropped");
        public double AverageClassScore => Students
            .Where(s => s.AverageScore.HasValue)
            .Select(s => s.AverageScore!.Value)
            .DefaultIfEmpty(0)
            .Average();
    }

    // ViewModel cho sinh viên trong lớp
    public class StudentInClassViewModel
    {
        public int EnrollmentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public float? MidtermScore { get; set; }
        public float? FinalScore { get; set; }
        public float? AverageScore { get; set; }
        public string? LetterGrade { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Notes { get; set; }
    }
}

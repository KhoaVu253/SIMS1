using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    // ViewModel cho việc phân công sinh viên vào lớp
    public class AssignStudentToCourseViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int CourseId { get; set; }

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
    }

    // ViewModel cho việc phân công hàng loạt theo khoa
    public class BulkAssignViewModel
    {
        [Required]
        [Display(Name = "Môn học")]
        public int CourseId { get; set; }

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
}

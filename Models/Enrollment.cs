using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        /// <summary>
        /// ✅ NEW: Lớp/lịch học cụ thể mà sinh viên được phân công vào
        /// Optional: Nếu null = Chỉ đăng ký môn, không gắn lớp cụ thể
        /// </summary>
        public int? ScheduleId { get; set; }

        [Required]
        [StringLength(20)]
        public string Semester { get; set; } = string.Empty; // "HK1", "HK2", "HK3"

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty; // "2024-2025"

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Failed, Retaking, Dropped

        // Grades
        public float? MidtermScore { get; set; }
        public float? FinalScore { get; set; }
        public float? AverageScore { get; set; }
        public string? LetterGrade { get; set; }

        // ✅ NEW: Fail/Retake tracking
        /// <summary>
        /// True nếu môn học bị trượt (AverageScore < 5.0)
        /// </summary>
        public bool IsFailed { get; set; } = false;

        /// <summary>
        /// True nếu đang học lại môn đã trượt
        /// </summary>
        public bool IsRetaking { get; set; } = false;

        /// <summary>
        /// ID của enrollment gốc (nếu đây là enrollment học lại)
        /// </summary>
        public int? OriginalEnrollmentId { get; set; }

        /// <summary>
        /// Số lần đã học lại môn này (0 = lần đầu, 1 = học lại lần 1, ...)
        /// </summary>
        public int RetakeCount { get; set; } = 0;

        // Assigned by Admin (không phải sinh viên tự đăng ký)
        public int? AssignedByUserId { get; set; }
        public DateTime? AssignedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú của Admin khi phân công

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public CourseSchedule? Schedule { get; set; } // ✅ NEW: Lớp cụ thể
        public User? AssignedByUser { get; set; }
        
        // ✅ NEW: Self-referencing for retake
        public Enrollment? OriginalEnrollment { get; set; }
    }
}

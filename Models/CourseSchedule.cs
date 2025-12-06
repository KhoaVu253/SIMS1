using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    /// <summary>
    /// Lịch học cho từng môn - Một môn có thể có nhiều buổi học trong tuần
    /// </summary>
    public class CourseSchedule
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        /// <summary>
        /// Giảng viên phụ trách buổi học này
        /// </summary>
        [Required]
        public int FacultyId { get; set; }

        [Required]
        [StringLength(20)]
        public string Semester { get; set; } = string.Empty; // HK1, HK2, HK3

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty; // 2024-2025

        /// <summary>
        /// Thứ trong tuần: 2=Thứ 2, 3=Thứ 3, ..., 8=Chủ nhật
        /// </summary>
        [Required]
        [Range(2, 8, ErrorMessage = "Thứ phải từ 2 (Thứ hai) đến 8 (Chủ nhật)")]
        public int DayOfWeek { get; set; }

        /// <summary>
        /// Tiết bắt đầu (1-12)
        /// Sáng: 1-6, Chiều: 7-12
        /// </summary>
        [Required]
        [Range(1, 12, ErrorMessage = "Tiết học từ 1 đến 12")]
        public int StartPeriod { get; set; }

        /// <summary>
        /// Tiết kết thúc (1-12)
        /// </summary>
        [Required]
        [Range(1, 12, ErrorMessage = "Tiết học từ 1 đến 12")]
        public int EndPeriod { get; set; }

        /// <summary>
        /// Phòng học (A101, B205, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Room { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày tạo lịch
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Course Course { get; set; } = null!;
        public Faculty Faculty { get; set; } = null!;
    }
}

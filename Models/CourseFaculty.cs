using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    /// <summary>
    /// Bảng trung gian: Liên kết giữa Course và Faculty
    /// Một môn học có thể có nhiều giảng viên, một giảng viên có thể dạy nhiều môn
    /// </summary>
    public class CourseFaculty
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int FacultyId { get; set; }

        /// <summary>
        /// Vai trò của giảng viên trong môn học
        /// VD: "Giảng viên chính", "Giảng viên phụ", "Trợ giảng"
        /// </summary>
        [StringLength(50)]
        public string Role { get; set; } = "Giảng viên";

        /// <summary>
        /// Lớp/nhóm mà giảng viên phụ trách
        /// VD: "Lớp A", "Lớp B", "Nhóm 1"
        /// </summary>
        [StringLength(50)]
        public string? ClassGroup { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái (có thể dùng để vô hiệu hóa phân công)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày phân công
        /// </summary>
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Course Course { get; set; } = null!;
        public Faculty Faculty { get; set; } = null!;
    }
}

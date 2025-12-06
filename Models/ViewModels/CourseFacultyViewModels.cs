using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form phân công giảng viên vào môn học
    /// </summary>
    public class AssignFacultyToCourseViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 giảng viên")]
        public List<int> FacultyIds { get; set; } = new();

        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "Giảng viên";

        [StringLength(50)]
        [Display(Name = "Lớp/Nhóm")]
        public string? ClassGroup { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel hiển thị danh sách giảng viên của môn học
    /// </summary>
    public class CourseFacultyListViewModel
    {
        public int CourseFacultyId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ClassGroup { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedDate { get; set; }
    }

    /// <summary>
    /// ViewModel cho CourseFormViewModel với multiple faculties
    /// </summary>
    public class CourseFormViewModelV2 : CourseFormViewModel
    {
        /// <summary>
        /// Danh sách giảng viên đã được phân công
        /// </summary>
        public List<int> AssignedFacultyIds { get; set; } = new();

        /// <summary>
        /// Danh sách giảng viên với thông tin chi tiết
        /// </summary>
        public List<CourseFacultyListViewModel> AssignedFaculties { get; set; } = new();
    }
}

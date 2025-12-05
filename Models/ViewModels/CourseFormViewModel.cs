using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    public class CourseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã môn học là bắt buộc")]
        [Display(Name = "Mã môn học")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên môn học là bắt buộc")]
        [Display(Name = "Tên môn học")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ từ 1 đến 10")]
        [Display(Name = "Số tín chỉ")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Khoa là bắt buộc")]
        [Display(Name = "Khoa")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Giảng viên phụ trách")]
        public int? FacultyId { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}

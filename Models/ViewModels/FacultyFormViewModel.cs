using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    public class FacultyFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã giảng viên là bắt buộc")]
        [Display(Name = "Mã giảng viên")]
        public string FacultyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khoa là bắt buộc")]
        [Display(Name = "Khoa")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}

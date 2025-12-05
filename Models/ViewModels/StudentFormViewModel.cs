using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    public class StudentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [Display(Name = "Mã sinh viên")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

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

        [Display(Name = "Lớp")]
        public string ClassName { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    public class AccountManagementViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;
        
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class BulkResetPasswordViewModel
    {
        [Display(Name = "Danh sách User IDs")]
        public List<int> UserIds { get; set; } = new();

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu mới cho tất cả")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

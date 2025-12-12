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
        
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;
        
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Password confirmation does not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class BulkResetPasswordViewModel
    {
        [Display(Name = "User ID List")]
        public List<int> UserIds { get; set; } = new();

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "New Password for All")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    public class FacultyFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Faculty code is required")]
        [Display(Name = "Faculty Code")]
        public string FacultyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}

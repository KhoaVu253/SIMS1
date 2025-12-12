using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    public class CourseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course code is required")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course name is required")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credits is required")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        [Display(Name = "Credits")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Assigned Faculty")]
        public int? FacultyId { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}

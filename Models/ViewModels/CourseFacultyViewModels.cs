using System.ComponentModel.DataAnnotations;

namespace SIMS.Models.ViewModels
{
    /// <summary>
    /// ViewModel for assigning faculty to course form
    /// </summary>
    public class AssignFacultyToCourseViewModel
    {
        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select at least 1 faculty")]
        public List<int> FacultyIds { get; set; } = new();

        [StringLength(50)]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Faculty";

        [StringLength(50)]
        [Display(Name = "Class/Group")]
        public string? ClassGroup { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel to display list of faculty for a course
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
    /// ViewModel for CourseFormViewModel with multiple faculties
    /// </summary>
    public class CourseFormViewModelV2 : CourseFormViewModel
    {
        /// <summary>
        /// List of assigned faculty IDs
        /// </summary>
        public List<int> AssignedFacultyIds { get; set; } = new();

        /// <summary>
        /// List of faculty with detailed information
        /// </summary>
        public List<CourseFacultyListViewModel> AssignedFaculties { get; set; } = new();
    }
}

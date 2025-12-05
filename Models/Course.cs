using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        public int Credits { get; set; }

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public int? FacultyId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Faculty? Faculty { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}

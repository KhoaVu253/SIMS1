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

        // ⚠️ DEPRECATED: Giữ lại để backward compatibility, sẽ remove sau
        // Dùng CourseFaculties thay thế
        [Obsolete("Use CourseFaculties navigation property instead")]
        public int? FacultyId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [Obsolete("Use CourseFaculties navigation property instead")]
        public Faculty? Faculty { get; set; }
        
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        
        // ✅ NEW: Many-to-Many relationship với Faculty
        public ICollection<CourseFaculty> CourseFaculties { get; set; } = new List<CourseFaculty>();
    }
}

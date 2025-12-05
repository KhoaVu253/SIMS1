using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Faculty
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string FacultyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

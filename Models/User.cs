using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // "Admin", "Student", "Faculty"

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Student? Student { get; set; }
        public Faculty? Faculty { get; set; }
    }
}

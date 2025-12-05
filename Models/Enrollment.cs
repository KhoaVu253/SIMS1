using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(20)]
        public string Semester { get; set; } = string.Empty; // "HK1", "HK2", "HK3"

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty; // "2024-2025"

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Dropped

        // Grades
        public float? MidtermScore { get; set; }
        public float? FinalScore { get; set; }
        public float? AverageScore { get; set; }
        public string? LetterGrade { get; set; }

        // Assigned by Admin (không phải sinh viên tự đăng ký)
        public int? AssignedByUserId { get; set; }
        public DateTime? AssignedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú của Admin khi phân công

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public User? AssignedByUser { get; set; }
    }
}

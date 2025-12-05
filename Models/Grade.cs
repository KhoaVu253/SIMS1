using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int EnrollmentId { get; set; }

        public float? MidtermScore { get; set; }

        public float? FinalScore { get; set; }

        public float? TotalScore { get; set; }

        [StringLength(5)]
        public string? LetterGrade { get; set; }

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public Enrollment Enrollment { get; set; } = null!;
    }
}

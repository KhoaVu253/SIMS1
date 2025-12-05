namespace SIMS.Models.ViewModels
{
    public class GradeEntryViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public List<StudentGradeEntry> Students { get; set; } = new List<StudentGradeEntry>();
    }

    public class StudentGradeEntry
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public float? MidtermScore { get; set; }
        public float? FinalScore { get; set; }
        public float? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
    }
}

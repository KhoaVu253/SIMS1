namespace SIMS.Models.ViewModels
{
    public class EnrollCourseViewModel
    {
        public List<Course> AvailableCourses { get; set; } = new List<Course>();
        public List<int> EnrolledCourseIds { get; set; } = new List<int>();
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
    }
}

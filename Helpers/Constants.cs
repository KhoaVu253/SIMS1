namespace SIMS.Helpers
{
    public static class Constants
    {
        // Roles
        public const string AdminRole = "Admin";
        public const string StudentRole = "Student";
        public const string FacultyRole = "Faculty";

        // Default Passwords
        public const string DefaultPassword = "123456";

        // Grade Weights
        public const float MidtermWeight = 0.4f;
        public const float FinalWeight = 0.6f;

        // Current Semester (can be loaded from config or database)
        public const string CurrentSemester = "HK1";
        public const string CurrentAcademicYear = "2024-2025";

        // Semesters
        public static readonly string[] Semesters = new[]
        {
            "HK1", // Semester 1
            "HK2", // Semester 2
            "HK3"  // Summer Semester
        };

        // Enrollment Status
        public static readonly string[] EnrollmentStatuses = new[]
        {
            "Active",    // Active
            "Completed", // Completed
            "Dropped"    // Dropped
        };

        // Departments - Fixed 3 departments
        public static readonly string[] Departments = new[]
        {
            "Information Technology",
            "Economics",
            "Graphic Design"
        };

        // Grade Scale
        public static readonly Dictionary<string, (float Min, float Max)> GradeScale = new()
        {
            { "A+", (9.0f, 10.0f) },
            { "A", (8.5f, 8.99f) },
            { "B+", (8.0f, 8.49f) },
            { "B", (7.0f, 7.99f) },
            { "C+", (6.5f, 6.99f) },
            { "C", (5.5f, 6.49f) },
            { "D+", (5.0f, 5.49f) },
            { "D", (4.0f, 4.99f) },
            { "F", (0.0f, 3.99f) }
        };

        // Validation Rules
        public const int MinCredits = 1;
        public const int MaxCredits = 10;
        public const float MinScore = 0;
        public const float MaxScore = 10;

        // UI Messages
        public const string SuccessMessage = "Operation successful!";
        public const string ErrorMessage = "An error occurred. Please try again!";
        public const string DeleteConfirmMessage = "Are you sure you want to delete?";
        public const string SaveConfirmMessage = "Are you sure you want to save?";
    }
}

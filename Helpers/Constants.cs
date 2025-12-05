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

        // Current Semester (có thể lấy từ config hoặc database)
        public const string CurrentSemester = "HK1";
        public const string CurrentAcademicYear = "2024-2025";

        // Semesters
        public static readonly string[] Semesters = new[]
        {
            "HK1", // Học kỳ 1
            "HK2", // Học kỳ 2
            "HK3"  // Học kỳ hè
        };

        // Enrollment Status
        public static readonly string[] EnrollmentStatuses = new[]
        {
            "Active",    // Đang học
            "Completed", // Hoàn thành
            "Dropped"    // Rút môn
        };

        // Departments - Fixed 3 departments
        public static readonly string[] Departments = new[]
        {
            "Công Nghệ Thông Tin",
            "Kinh Tế",
            "Thiết Kế Đồ Họa"
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
        public const string SuccessMessage = "Thao tác thành công!";
        public const string ErrorMessage = "Có lỗi xảy ra. Vui lòng thử lại!";
        public const string DeleteConfirmMessage = "Bạn có chắc chắn muốn xóa?";
        public const string SaveConfirmMessage = "Bạn có chắc chắn muốn lưu?";
    }
}

using System.Security.Claims;

namespace SIMS.Helpers
{
    public static class Extensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole(Constants.AdminRole);
        }

        public static bool IsStudent(this ClaimsPrincipal user)
        {
            return user.IsInRole(Constants.StudentRole);
        }

        public static bool IsFaculty(this ClaimsPrincipal user)
        {
            return user.IsInRole(Constants.FacultyRole);
        }

        public static string ToVietnameseDate(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public static string ToVietnameseDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static string GetBadgeClass(this string letterGrade)
        {
            return letterGrade switch
            {
                "A+" or "A" or "B+" or "B" => "bg-success",
                "C+" or "C" => "bg-warning",
                "D+" or "D" => "bg-info",
                "F" => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}

using SIMS.Models;
using Microsoft.EntityFrameworkCore;

namespace SIMS.Services
{
    public interface IGradeCalculationService
    {
        float CalculateTotalScore(float? midterm, float? final);
        string CalculateLetterGrade(float totalScore);
        float CalculateGPA(IEnumerable<Grade> grades);
    }

    public class GradeCalculationService : IGradeCalculationService
    {
        public float CalculateTotalScore(float? midterm, float? final)
        {
            if (!midterm.HasValue || !final.HasValue)
                return 0;

            return (midterm.Value * 0.4f) + (final.Value * 0.6f);
        }

        public string CalculateLetterGrade(float totalScore)
        {
            return totalScore switch
            {
                >= 9.0f => "A+",
                >= 8.5f => "A",
                >= 8.0f => "B+",
                >= 7.0f => "B",
                >= 6.5f => "C+",
                >= 5.5f => "C",
                >= 5.0f => "D+",
                >= 4.0f => "D",
                _ => "F"
            };
        }

        public float CalculateGPA(IEnumerable<Grade> grades)
        {
            var validGrades = grades.Where(g => g.TotalScore.HasValue).ToList();
            
            if (!validGrades.Any())
                return 0;

            return validGrades.Average(g => g.TotalScore!.Value);
        }
    }
}

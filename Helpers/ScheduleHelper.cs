namespace SIMS.Helpers
{
    /// <summary>
    /// Helper class for schedule management
    /// </summary>
    public static class ScheduleHelper
    {
        // =============================================
        // DAY CONVERSION
        // =============================================

        /// <summary>
        /// Convert number to full day name
        /// </summary>
        public static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                2 => "Monday",
                3 => "Tuesday",
                4 => "Wednesday",
                5 => "Thursday",
                6 => "Friday",
                7 => "Saturday",
                8 => "Sunday",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Convert number to abbreviated day name
        /// </summary>
        public static string GetDayAbbreviation(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                2 => "Mon",
                3 => "Tue",
                4 => "Wed",
                5 => "Thu",
                6 => "Fri",
                7 => "Sat",
                8 => "Sun",
                _ => "?"
            };
        }

        /// <summary>
        /// Get list of all days
        /// </summary>
        public static List<(int Value, string Name)> GetAllDays()
        {
            return new List<(int, string)>
            {
                (2, "Monday"),
                (3, "Tuesday"),
                (4, "Wednesday"),
                (5, "Thursday"),
                (6, "Friday"),
                (7, "Saturday"),
                (8, "Sunday")
            };
        }

        // =============================================
        // PERIOD/TIME CONVERSION
        // =============================================

        /// <summary>
        /// Get start time of period
        /// </summary>
        public static string GetPeriodStartTime(int period)
        {
            return period switch
            {
                1 => "07:00",
                2 => "07:50",
                3 => "08:50",
                4 => "09:40",
                5 => "10:40",
                6 => "11:30",
                7 => "13:00",
                8 => "13:50",
                9 => "14:50",
                10 => "15:40",
                11 => "16:40",
                12 => "17:30",
                _ => "??"
            };
        }

        /// <summary>
        /// Get end time of period
        /// </summary>
        public static string GetPeriodEndTime(int period)
        {
            return period switch
            {
                1 => "07:50",
                2 => "08:40",
                3 => "09:40",
                4 => "10:30",
                5 => "11:30",
                6 => "12:20",
                7 => "13:50",
                8 => "14:40",
                9 => "15:40",
                10 => "16:30",
                11 => "17:30",
                12 => "18:20",
                _ => "??"
            };
        }

        /// <summary>
        /// Get time range (e.g., "07:00 - 09:30")
        /// </summary>
        public static string GetTimeRange(int startPeriod, int endPeriod)
        {
            return $"{GetPeriodStartTime(startPeriod)} - {GetPeriodEndTime(endPeriod)}";
        }

        /// <summary>
        /// Get period range (e.g., "Period 1-3")
        /// </summary>
        public static string GetPeriodRange(int startPeriod, int endPeriod)
        {
            if (startPeriod == endPeriod)
                return $"Period {startPeriod}";
            return $"Period {startPeriod}-{endPeriod}";
        }

        /// <summary>
        /// Get list of all periods
        /// </summary>
        public static List<int> GetAllPeriods()
        {
            return Enumerable.Range(1, 12).ToList();
        }

        // =============================================
        // SESSION CLASSIFICATION
        // =============================================

        /// <summary>
        /// Classify session type (Morning/Afternoon)
        /// </summary>
        public static string GetSessionType(int startPeriod)
        {
            return startPeriod <= 6 ? "Morning" : "Afternoon";
        }

        /// <summary>
        /// Get CSS color class for session
        /// </summary>
        public static string GetSessionColorClass(int startPeriod)
        {
            return startPeriod <= 6 ? "bg-info-subtle" : "bg-warning-subtle";
        }

        /// <summary>
        /// Get badge color class for session
        /// </summary>
        public static string GetSessionBadgeClass(int startPeriod)
        {
            return startPeriod <= 6 ? "badge bg-info" : "badge bg-warning";
        }

        // =============================================
        // CONFLICT DETECTION
        // =============================================

        /// <summary>
        /// Check if two schedules have time conflict
        /// </summary>
        public static bool IsTimeConflict(
            int day1, int start1, int end1,
            int day2, int start2, int end2)
        {
            // Different days = no conflict
            if (day1 != day2) return false;

            // Check period overlap
            // No conflict if: schedule 1 ends before schedule 2 starts
            // or schedule 1 starts after schedule 2 ends
            return !(end1 < start2 || start1 > end2);
        }

        /// <summary>
        /// Calculate number of periods
        /// </summary>
        public static int GetPeriodCount(int startPeriod, int endPeriod)
        {
            return endPeriod - startPeriod + 1;
        }

        // =============================================
        // VALIDATION
        // =============================================

        /// <summary>
        /// Validate schedule
        /// </summary>
        public static List<string> ValidateSchedule(int dayOfWeek, int startPeriod, int endPeriod)
        {
            var errors = new List<string>();

            if (dayOfWeek < 2 || dayOfWeek > 8)
                errors.Add("Day of week must be between 2 (Monday) and 8 (Sunday)");

            if (startPeriod < 1 || startPeriod > 12)
                errors.Add("Start period must be between 1 and 12");

            if (endPeriod < 1 || endPeriod > 12)
                errors.Add("End period must be between 1 and 12");

            if (endPeriod < startPeriod)
                errors.Add("End period must be greater than or equal to start period");

            // Should not exceed 5 consecutive periods
            if (endPeriod - startPeriod + 1 > 5)
                errors.Add("Should not schedule more than 5 consecutive periods");

            return errors;
        }

        // =============================================
        // DISPLAY HELPERS
        // =============================================

        /// <summary>
        /// Format schedule info briefly
        /// </summary>
        public static string GetScheduleSummary(int dayOfWeek, int startPeriod, int endPeriod, string room)
        {
            return $"{GetDayAbbreviation(dayOfWeek)}, {GetPeriodRange(startPeriod, endPeriod)}, {room}";
        }

        /// <summary>
        /// Get icon for session
        /// </summary>
        public static string GetSessionIcon(int startPeriod)
        {
            return startPeriod <= 6 ? "bi-sunrise" : "bi-sunset";
        }
    }
}

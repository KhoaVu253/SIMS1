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
        /// Chuyển số thành tên thứ đầy đủ
        /// </summary>
        public static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                2 => "Thứ hai",
                3 => "Thứ ba",
                4 => "Thứ tư",
                5 => "Thứ năm",
                6 => "Thứ sáu",
                7 => "Thứ bảy",
                8 => "Chủ nhật",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Chuyển số thành tên thứ viết tắt
        /// </summary>
        public static string GetDayAbbreviation(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                2 => "T2",
                3 => "T3",
                4 => "T4",
                5 => "T5",
                6 => "T6",
                7 => "T7",
                8 => "CN",
                _ => "?"
            };
        }

        /// <summary>
        /// Lấy danh sách tất cả các thứ
        /// </summary>
        public static List<(int Value, string Name)> GetAllDays()
        {
            return new List<(int, string)>
            {
                (2, "Thứ hai"),
                (3, "Thứ ba"),
                (4, "Thứ tư"),
                (5, "Thứ năm"),
                (6, "Thứ sáu"),
                (7, "Thứ bảy"),
                (8, "Chủ nhật")
            };
        }

        // =============================================
        // PERIOD/TIME CONVERSION
        // =============================================

        /// <summary>
        /// Lấy giờ bắt đầu của tiết học
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
        /// Lấy giờ kết thúc của tiết học
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
        /// Lấy khoảng thời gian học (VD: "07:00 - 09:30")
        /// </summary>
        public static string GetTimeRange(int startPeriod, int endPeriod)
        {
            return $"{GetPeriodStartTime(startPeriod)} - {GetPeriodEndTime(endPeriod)}";
        }

        /// <summary>
        /// Lấy khoảng tiết học (VD: "Tiết 1-3")
        /// </summary>
        public static string GetPeriodRange(int startPeriod, int endPeriod)
        {
            if (startPeriod == endPeriod)
                return $"Tiết {startPeriod}";
            return $"Tiết {startPeriod}-{endPeriod}";
        }

        /// <summary>
        /// Lấy danh sách tất cả các tiết
        /// </summary>
        public static List<int> GetAllPeriods()
        {
            return Enumerable.Range(1, 12).ToList();
        }

        // =============================================
        // SESSION CLASSIFICATION
        // =============================================

        /// <summary>
        /// Phân loại buổi học (Sáng/Chiều)
        /// </summary>
        public static string GetSessionType(int startPeriod)
        {
            return startPeriod <= 6 ? "Sáng" : "Chiều";
        }

        /// <summary>
        /// Lấy CSS class màu sắc cho buổi học
        /// </summary>
        public static string GetSessionColorClass(int startPeriod)
        {
            return startPeriod <= 6 ? "bg-info-subtle" : "bg-warning-subtle";
        }

        /// <summary>
        /// Lấy màu badge cho buổi học
        /// </summary>
        public static string GetSessionBadgeClass(int startPeriod)
        {
            return startPeriod <= 6 ? "badge bg-info" : "badge bg-warning";
        }

        // =============================================
        // CONFLICT DETECTION
        // =============================================

        /// <summary>
        /// Kiểm tra 2 lịch có trùng thời gian không
        /// </summary>
        public static bool IsTimeConflict(
            int day1, int start1, int end1,
            int day2, int start2, int end2)
        {
            // Khác ngày thì không trùng
            if (day1 != day2) return false;

            // Kiểm tra trùng tiết
            // Không trùng nếu: lịch 1 kết thúc trước khi lịch 2 bắt đầu
            // hoặc lịch 1 bắt đầu sau khi lịch 2 kết thúc
            return !(end1 < start2 || start1 > end2);
        }

        /// <summary>
        /// Tính số tiết học
        /// </summary>
        public static int GetPeriodCount(int startPeriod, int endPeriod)
        {
            return endPeriod - startPeriod + 1;
        }

        // =============================================
        // VALIDATION
        // =============================================

        /// <summary>
        /// Validate lịch học
        /// </summary>
        public static List<string> ValidateSchedule(int dayOfWeek, int startPeriod, int endPeriod)
        {
            var errors = new List<string>();

            if (dayOfWeek < 2 || dayOfWeek > 8)
                errors.Add("Thứ phải từ 2 (Thứ hai) đến 8 (Chủ nhật)");

            if (startPeriod < 1 || startPeriod > 12)
                errors.Add("Tiết bắt đầu phải từ 1 đến 12");

            if (endPeriod < 1 || endPeriod > 12)
                errors.Add("Tiết kết thúc phải từ 1 đến 12");

            if (endPeriod < startPeriod)
                errors.Add("Tiết kết thúc phải lớn hơn hoặc bằng tiết bắt đầu");

            // Không nên học quá 5 tiết liên tục
            if (endPeriod - startPeriod + 1 > 5)
                errors.Add("Không nên sắp xếp quá 5 tiết học liên tục");

            return errors;
        }

        // =============================================
        // DISPLAY HELPERS
        // =============================================

        /// <summary>
        /// Format thông tin lịch học ngắn gọn
        /// </summary>
        public static string GetScheduleSummary(int dayOfWeek, int startPeriod, int endPeriod, string room)
        {
            return $"{GetDayAbbreviation(dayOfWeek)}, {GetPeriodRange(startPeriod, endPeriod)}, {room}";
        }

        /// <summary>
        /// Lấy icon cho buổi học
        /// </summary>
        public static string GetSessionIcon(int startPeriod)
        {
            return startPeriod <= 6 ? "bi-sunrise" : "bi-sunset";
        }
    }
}

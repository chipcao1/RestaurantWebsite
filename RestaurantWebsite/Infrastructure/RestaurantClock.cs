using System;
namespace RestaurantWebsite.Infrastructure
{
    public static class RestaurantClock
    {
        // Dữ liệu DateTime hiện có dùng giờ địa phương Việt Nam (không chuyển đổi lịch sử).
        public static DateTime Now { get { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone); } }
        private static TimeZoneInfo Zone
        {
            get
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
                catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
            }
        }
    }
}

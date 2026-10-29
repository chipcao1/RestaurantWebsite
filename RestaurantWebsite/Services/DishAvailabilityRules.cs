using System;

namespace RestaurantWebsite.Services
{
    public static class DishAvailabilityRules
    {
        public static bool CanToggle(string status)
        {
            return status == "Available" || status == "Temporarily Unavailable" || status == "Unavailable";
        }

        public static string NextStatus(string status)
        {
            if (!CanToggle(status))
                throw new InvalidOperationException("Món đã ngừng kinh doanh hoặc không có trạng thái phục vụ hợp lệ.");
            return status == "Available" ? "Temporarily Unavailable" : "Available";
        }
    }
}

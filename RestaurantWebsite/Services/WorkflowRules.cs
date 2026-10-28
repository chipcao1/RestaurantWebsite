using System;
namespace RestaurantWebsite.Services
{
    /// <summary>Quy tắc trạng thái dùng chung cho mọi vai trò; không phụ thuộc HTTP hoặc DB.</summary>
    public static class WorkflowRules
    {
        public static bool IsOperator(string role) { return role == "Admin" || role == "Staff"; }
        public static bool OrderTransition(string current, string next, string role, bool paid)
        {
            if (current == next || current == "Cancelled" || current == "Delivered") return false;
            if (role == "Customer") return current == "Pending" && next == "Cancelled" && !paid;
            if (!IsOperator(role)) return false;
            if (next == "Cancelled") return (current == "Pending" || current == "Confirmed") && !paid;
            return (current == "Pending" && next == "Confirmed") ||
                (current == "Confirmed" && next == "Preparing") ||
                (current == "Preparing" && next == "Ready") ||
                (current == "Ready" && next == "Delivered" && paid);
        }
        public static bool PaymentTransition(string current, string next)
        {
            return ((current == "Pending" || current == "Failed") && (next == "Completed" || (current == "Pending" && next == "Failed"))) ||
                (current == "Completed" && next == "Refunded");
        }
        public static bool ReservationTransition(string current, string next, string role, DateTime start, DateTime now, bool hasOpenOrders)
        {
            if ((current != "Pending" && current != "Confirmed") || current == next) return false;
            if (role == "Customer") return next == "Cancelled" && start > now && !hasOpenOrders;
            if (!IsOperator(role)) return false;
            if (next == "Cancelled") return !hasOpenOrders;
            if (next == "Confirmed") return current == "Pending" && start.AddHours(2) > now;
            return current == "Confirmed" && next == "Completed" && start <= now && !hasOpenOrders;
        }
        public static bool Overlaps(DateTime start, DateTime end, DateTime otherStart, DateTime otherEnd)
        { return start < otherEnd && end > otherStart; }
    }
}

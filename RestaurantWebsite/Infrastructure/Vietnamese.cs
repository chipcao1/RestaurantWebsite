using System;
using System.Collections.Generic;
using System.Globalization;
namespace RestaurantWebsite.Infrastructure
{
    public static class Vietnamese
    {
        public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("vi-VN");
        private static readonly IDictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"Pending", "Chờ xác nhận"}, {"Confirmed", "Đã xác nhận"}, {"Preparing", "Đang chế biến"},
            {"Ready", "Sẵn sàng phục vụ"}, {"Delivered", "Hoàn tất"}, {"Cancelled", "Đã hủy"},
            {"Completed", "Hoàn tất"}, {"Failed", "Không thành công"}, {"Refunded", "Đã hoàn tiền"},
            {"Delivery", "Giao hàng"}, {"Dine-in", "Tại bàn"}, {"Takeaway", "Mang về"},
            {"Cash", "Tiền mặt"}, {"Bank Transfer", "Chuyển khoản"}, {"Digital Wallet", "Ví điện tử"},
            {"Admin", "Quản trị viên"}, {"Staff", "Nhân viên"}, {"Customer", "Khách hàng"},
            {"Available", "Sẵn sàng"}, {"Unavailable", "Tạm ngừng phục vụ"}, {"Temporarily Unavailable", "Tạm ngừng phục vụ"},
            {"Occupied", "Đang sử dụng"}, {"Reserved", "Đã đặt"}, {"Maintenance", "Bảo trì"},
            {"Active", "Đang hoạt động"}, {"Inactive", "Ngừng hoạt động"}, {"Expired", "Hết hạn"},
            {"Upcoming", "Sắp áp dụng"}, {"All", "Tất cả"}, {"Percentage", "Phần trăm"}, {"Percent", "Phần trăm"},
            {"FixedAmount", "Số tiền"}, {"Fixed", "Số tiền"}, {"Fixed Amount", "Số tiền"}, {"Amount", "Số tiền"}, {"Discontinued", "Ngừng kinh doanh"},
            {"Window", "Gần cửa sổ"}, {"Indoor", "Trong nhà"}, {"Outdoor", "Ngoài trời"}, {"VIP", "Phòng riêng"}
        };
        public static string Label(string value) { string label; return value != null && Labels.TryGetValue(value, out label) ? label : value ?? ""; }
        public static string Money(decimal value) { return value.ToString("N0", Culture) + " ₫"; }
    }
}

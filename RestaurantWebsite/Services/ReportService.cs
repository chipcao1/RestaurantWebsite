using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using RestaurantWebsite.Models;
using RestaurantWebsite.Infrastructure;
namespace RestaurantWebsite.Services
{
    public sealed class SalesTransaction
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal Collected { get; set; }
        public decimal Refunded { get; set; }
        public decimal Net { get { return Collected - Refunded; } }
    }
    public sealed class SalesReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal Collected { get; set; }
        public decimal Refunded { get; set; }
        public decimal Net { get { return Collected - Refunded; } }
        public IList<string> Lines { get; set; }
        public IList<SalesTransaction> Transactions { get; set; }
    }
    public sealed class ReportService
    {
        private readonly AppDbContext db;
        public ReportService(AppDbContext context) { db = context; }
        public SalesReport Sales(DateTime from, DateTime to)
        {
            from = from.Date; to = to.Date;
            if (to < from || (to - from).TotalDays > 366) throw new InvalidOperationException("Khoảng báo cáo phải hợp lệ và không quá 366 ngày.");
            var end = to.AddDays(1);
            var collected = db.Payments.Where(p => (p.Status == "Completed" || p.Status == "Refunded") && p.PaymentDate >= from && p.PaymentDate < end).ToList();
            var refunded = collected.Where(p => p.Status == "Refunded").ToList();
            var totals = ReportMath.Calculate(collected, from, end);
            return new SalesReport { From = from, To = to, Collected = totals.Collected, Refunded = totals.Refunded, Transactions = collected.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id).Select(p => new SalesTransaction { PaymentId = p.Id, OrderId = p.OrderId, PaymentDate = p.PaymentDate, PaymentMethod = p.PaymentMethod, Status = p.Status, Collected = p.Amount, Refunded = p.Status == "Refunded" ? p.Amount : 0m }).ToList(), Lines = collected.Select(p => new { At = p.PaymentDate, Text = p.PaymentDate.ToString("dd/MM/yyyy HH:mm") + " | Thu đơn #" + p.OrderId + " | " + Vietnamese.Money(p.Amount) }).Concat(refunded.Select(p => new { At = p.PaymentDate, Text = p.PaymentDate.ToString("dd/MM/yyyy HH:mm") + " | Đã hoàn (ngày thanh toán gốc) đơn #" + p.OrderId + " | -" + Vietnamese.Money(p.Amount) })).OrderBy(x => x.At).Select(x => x.Text).ToList() };
        }
        public IList<string> Menu()
        {
            return db.Dishes.Include(d => d.Category).OrderBy(d => d.Category.Name).ThenBy(d => d.Name).ToList().Select(d => d.Category.Name + " | " + d.Name + " | " + Vietnamese.Money(d.Price) + " | " + Vietnamese.Label(d.Status)).ToList();
        }
        public IList<string> Receipt(Order order)
        {
            var lines = new List<string> { "HÓA ĐƠN NHÀ HÀNG FEANE", "Mã đơn: #" + order.Id, "Ngày: " + order.CreatedAt.ToString("dd/MM/yyyy HH:mm"), "Khách hàng: " + order.User.FullName, "Hình thức: " + Vietnamese.Label(order.OrderType), "Trạng thái: " + Vietnamese.Label(order.Status) };
            foreach (var i in order.OrderItems) lines.Add((i.Dish != null ? i.Dish.Name : i.Combo != null ? i.Combo.Name : "Món ăn") + " | " + i.Quantity + " x " + Vietnamese.Money(i.UnitPrice) + " = " + Vietnamese.Money(i.SubTotal));
            lines.Add("Tạm tính: " + Vietnamese.Money(order.SubTotal)); lines.Add("Giảm giá: " + Vietnamese.Money(order.DiscountAmount)); lines.Add("Phí giao hàng: " + Vietnamese.Money(order.DeliveryFee)); lines.Add("TỔNG CỘNG: " + Vietnamese.Money(order.TotalAmount));
            foreach (var p in order.Payments) lines.Add("Thanh toán: " + Vietnamese.Label(p.PaymentMethod) + " — " + Vietnamese.Label(p.Status));
            lines.Add("Cảm ơn quý khách. Hẹn gặp lại!"); return lines;
        }
    }
}

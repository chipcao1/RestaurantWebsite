using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using RestaurantWebsite.Models;
using RestaurantWebsite.Infrastructure;
namespace RestaurantWebsite.Services
{
    public sealed class OrderWorkflowService
    {
        private readonly AppDbContext db;
        public OrderWorkflowService(AppDbContext context) { db = context; }
        public void ChangeStatus(int id, string next, int actorId, string role)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var order = db.Orders.Include(o => o.Payments).SingleOrDefault(o => o.Id == id);
                if (order == null || (role == "Customer" && order.UserId != actorId)) throw new InvalidOperationException("Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.");
                bool paid = order.Payments.Any(p => p.Status == "Completed");
                if (!WorkflowRules.OrderTransition(order.Status, next, role, paid)) throw new InvalidOperationException("Không thể chuyển trạng thái đơn hàng. Đơn đã thu tiền cần hoàn tiền trước khi hủy.");
                if (next == "Delivered" && order.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount) < order.TotalAmount) throw new InvalidOperationException("Đơn hàng chưa được thanh toán đủ.");
                order.Status = next;
                if (next == "Cancelled") foreach (var payment in order.Payments.Where(p => p.Status == "Pending")) payment.Status = "Failed";
                db.SaveChanges(); tx.Commit();
            }
        }
        public void ChangePayment(int paymentId, string next, int actorId, string transactionCode)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var payment = db.Payments.Include(p => p.Order).SingleOrDefault(p => p.Id == paymentId);
                if (payment == null || !WorkflowRules.PaymentTransition(payment.Status, next)) throw new InvalidOperationException("Trạng thái thanh toán không hợp lệ hoặc giao dịch đã được ghi nhận.");
                if (next == "Completed")
                {
                    if (payment.Order.Status == "Cancelled" || payment.Order.Status == "Delivered") throw new InvalidOperationException("Đơn hàng đã kết thúc.");
                    if (payment.Amount != payment.Order.TotalAmount || db.Payments.Any(p => p.OrderId == payment.OrderId && p.Id != payment.Id && p.Status == "Completed")) throw new InvalidOperationException("Số tiền không hợp lệ hoặc đơn đã được thanh toán.");
                    payment.PaymentDate = RestaurantClock.Now;
                    if (!string.IsNullOrWhiteSpace(transactionCode))
                    {
                        transactionCode = transactionCode.Trim();
                        if (transactionCode.Length > 100 || db.Payments.Any(p => p.Id != paymentId && p.TransactionCode == transactionCode)) throw new InvalidOperationException("Mã giao dịch không hợp lệ hoặc đã được dùng.");
                        payment.TransactionCode = transactionCode;
                    }
                }
                if (next == "Refunded")
                {
                    if (payment.Order.Status != "Pending" && payment.Order.Status != "Confirmed") throw new InvalidOperationException("Chỉ hoàn và hủy đơn trước khi chế biến.");
                    payment.Order.Status = "Cancelled";
                    foreach (var other in db.Payments.Where(p => p.OrderId == payment.OrderId && p.Status == "Pending")) other.Status = "Failed";
                }
                payment.Status = next; db.SaveChanges(); tx.Commit();
            }
        }
    }
}

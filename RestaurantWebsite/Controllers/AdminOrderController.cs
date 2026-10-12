using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminOrderController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Exact official statuses used in system
        private readonly string[] ValidOrderStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivered", "Cancelled" };
        private readonly string[] ValidPaymentStatuses = new[] { "Pending", "Completed", "Failed", "Refunded" };
        private readonly string[] ValidOrderTypes = new[] { "Delivery", "Dine-in", "Takeaway" };

        // =====================================================
        // GET: admin/orders (List & Filter)
        // =====================================================
        [HttpGet]
        public ActionResult Index(string status, string orderType, string date, string search)
        {
            var query = db.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Filter by Order Status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            // Filter by Order Type
            if (!string.IsNullOrWhiteSpace(orderType) && orderType != "All")
            {
                query = query.Where(o => o.OrderType == orderType);
            }

            // Filter by Date
            DateTime parsedDate;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out parsedDate))
            {
                DateTime startOfDay = parsedDate.Date;
                DateTime endOfDay = startOfDay.AddDays(1);
                query = query.Where(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay);
            }

            // Search by Order ID, Customer Name, Email, Phone or Address
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                int orderIdSearch;
                bool isNumeric = int.TryParse(search.Replace("#", ""), out orderIdSearch);

                query = query.Where(o =>
                    (isNumeric && o.Id == orderIdSearch) ||
                    o.User.FullName.ToLower().Contains(search) ||
                    o.User.Email.ToLower().Contains(search) ||
                    (o.Phone != null && o.Phone.ToLower().Contains(search)) ||
                    (o.DeliveryAddress != null && o.DeliveryAddress.ToLower().Contains(search)));
            }

            var orders = query
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            ViewBag.StatusList = new SelectList(ValidOrderStatuses, status);
            ViewBag.OrderTypeList = new SelectList(ValidOrderTypes, orderType);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SelectedOrderType = orderType ?? "All";
            ViewBag.SelectedDate = date;
            ViewBag.SearchTerm = search;

            return View("~/Views/Order/Index.cshtml", orders);
        }

        // =====================================================
        // GET: admin/orders/details/5
        // =====================================================
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Order ID was not provided.";
                return RedirectToAction("Index");
            }

            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.Reservation)
                .Include(o => o.OrderItems.Select(oi => oi.Dish))
                .Include(o => o.OrderItems.Select(oi => oi.Combo))
                .Include(o => o.Payments)
                .Include(o => o.OrderPromotions.Select(op => op.Promotion))
                .FirstOrDefault(o => o.Id == id.Value);

            if (order == null)
            {
                TempData["Error"] = "Order was not found.";
                return RedirectToAction("Index");
            }

            ViewBag.ValidOrderStatuses = ValidOrderStatuses;
            ViewBag.ValidPaymentStatuses = ValidPaymentStatuses;

            return View("~/Views/Order/Details.cshtml", order);
        }

        // =====================================================
        // POST: admin/orders/update-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string newStatus)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                TempData["Error"] = "Order was not found.";
                return RedirectToAction("Index");
            }

            if (!ValidOrderStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid order status specified.";
                return RedirectToAction("Details", new { id = id });
            }

            order.Status = newStatus;
            db.SaveChanges();

            TempData["Success"] = $"Order #{id} status updated to \"{newStatus}\".";
            return RedirectToAction("Details", new { id = id });
        }

        // =====================================================
        // POST: admin/orders/update-payment-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePaymentStatus(int paymentId, string newPaymentStatus)
        {
            var payment = db.Payments
                .Include(p => p.Order)
                .FirstOrDefault(p => p.Id == paymentId);

            if (payment == null)
            {
                TempData["Error"] = "Payment record was not found.";
                return RedirectToAction("Index");
            }

            if (!ValidPaymentStatuses.Contains(newPaymentStatus))
            {
                TempData["Error"] = "Invalid payment status specified.";
                return RedirectToAction("Details", new { id = payment.OrderId });
            }

            payment.Status = newPaymentStatus;
            if (newPaymentStatus == "Completed" && payment.PaymentDate == default(DateTime))
            {
                payment.PaymentDate = DateTime.Now;
            }

            db.SaveChanges();

            TempData["Success"] = $"Payment #{payment.Id} for Order #{payment.OrderId} updated to \"{newPaymentStatus}\".";
            return RedirectToAction("Details", new { id = payment.OrderId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

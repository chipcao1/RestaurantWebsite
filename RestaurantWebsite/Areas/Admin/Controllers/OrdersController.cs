using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class OrdersController : Controller
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

            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(ValidOrderStatuses, status);
            ViewBag.OrderTypeList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(ValidOrderTypes, orderType);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SelectedOrderType = orderType ?? "All";
            ViewBag.SelectedDate = date;
            ViewBag.SearchTerm = search;

            return View(orders);
        }

        // =====================================================
        // GET: admin/orders/details/5
        // =====================================================
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Thiếu mã đơn hàng.";
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
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            ViewBag.ValidOrderStatuses = ValidOrderStatuses;
            ViewBag.ValidPaymentStatuses = ValidPaymentStatuses;

            return View(order);
        }

        // =====================================================
        // POST: admin/orders/update-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string newStatus)
        {
            try { new RestaurantWebsite.Services.OrderWorkflowService(db).ChangeStatus(id, newStatus, (int)Session["UserId"], Convert.ToString(Session["Role"])); TempData["Success"] = "Đã cập nhật đơn hàng."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Details", new { id });
        }

        // =====================================================
        // POST: admin/orders/update-payment-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePaymentStatus(int paymentId, string newPaymentStatus)
        {
            try { new RestaurantWebsite.Services.OrderWorkflowService(db).ChangePayment(paymentId, newPaymentStatus, (int)Session["UserId"], Request.Form["transactionCode"]); TempData["Success"] = "Đã ghi nhận thanh toán."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Index");
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

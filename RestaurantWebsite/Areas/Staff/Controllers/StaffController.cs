using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
using RestaurantWebsite.Security;

namespace RestaurantWebsite.Areas.Staff.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class StaffController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly string[] ValidOrderStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivered", "Cancelled" };
        private readonly string[] ValidReservationStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
        private readonly string[] ValidPaymentStatuses = new[] { "Pending", "Completed", "Failed", "Refunded" };

        // =====================================================
        // GET: /staff (Dashboard)
        // =====================================================
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.CurrentRole = Session["Role"];
            ViewBag.UserName = Session["UserName"];

            DateTime todayStart = RestaurantWebsite.Infrastructure.RestaurantClock.Now.Date;
            DateTime tomorrowStart = todayStart.AddDays(1);

            // Real-time Staff KPIs
            ViewBag.PendingOrdersCount = db.Orders.Count(o => o.Status == "Pending");
            ViewBag.ProcessingOrdersCount = db.Orders.Count(o => o.Status == "Confirmed" || o.Status == "Preparing" || o.Status == "Ready");
            ViewBag.TodayReservationsCount = db.Reservations.Count(r => r.ReservationDate >= todayStart && r.ReservationDate < tomorrowStart);
            ViewBag.AvailableTablesCount = db.RestaurantTables.Count(t => t.IsActive && t.Status == "Available");

            // Recent Orders for quick staff dispatch
            var recentOrders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedAt)
                .Take(6)
                .ToList();

            // Today's Reservations
            var todayReservations = db.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .Where(r => r.ReservationDate >= todayStart && r.ReservationDate < tomorrowStart)
                .OrderBy(r => r.ReservationDate)
                .ToList();

            ViewBag.RecentOrders = recentOrders;
            ViewBag.TodayReservations = todayReservations;

            return View("~/Areas/Staff/Views/Staff/Index.cshtml");
        }

        // =====================================================
        // GET: /staff/dishes (Menu Availability Management)
        // =====================================================
        [HttpGet]
        public ActionResult Dishes(string search, string categoryId)
        {
            return RedirectToAction("Index", "Dishes", new { area = "Staff", search, categoryId });
        }

        // =====================================================
        // POST: /staff/toggle-dish-status/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleDishStatus(int id)
        {
            try
            {
                new RestaurantWebsite.Services.DishAvailabilityService(db).Toggle(id);
                TempData["Success"] = "Đã cập nhật tình trạng phục vụ món ăn.";
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index", "Dishes", new { area = "Staff" });
        }

        // =====================================================
        // GET: /staff/orders (Staff Order Queue & Processing)
        // =====================================================
        [HttpGet]
        public ActionResult Orders(string status, string search)
        {
            var query = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Dish))
                .Include(o => o.OrderItems.Select(oi => oi.Combo))
                .Include(o => o.Payments)
                .AsQueryable();

            if (status == "Processing") query = query.Where(o => o.Status == "Confirmed" || o.Status == "Preparing" || o.Status == "Ready");
            else if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                int orderIdSearch;
                bool isNumeric = int.TryParse(search.Replace("#", ""), out orderIdSearch);

                query = query.Where(o =>
                    (isNumeric && o.Id == orderIdSearch) ||
                    o.User.FullName.ToLower().Contains(search) ||
                    (o.Phone != null && o.Phone.ToLower().Contains(search)));
            }

            var orders = query.OrderByDescending(o => o.CreatedAt).ToList();

            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(ValidOrderStatuses, status);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;

            return View("~/Areas/Staff/Views/Staff/Orders.cshtml", orders);
        }

        // =====================================================
        // POST: /staff/update-order-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int id, string newStatus, string returnAction)
        {
            try { new RestaurantWebsite.Services.OrderWorkflowService(db).ChangeStatus(id, newStatus, (int)Session["UserId"], Convert.ToString(Session["Role"])); TempData["Success"] = "Đã cập nhật đơn hàng."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Orders");
        }

        // =====================================================
        // POST: /staff/update-payment-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePaymentStatus(int paymentId, string newPaymentStatus)
        {
            try { new RestaurantWebsite.Services.OrderWorkflowService(db).ChangePayment(paymentId, newPaymentStatus, (int)Session["UserId"], Request.Form["transactionCode"]); TempData["Success"] = "Đã ghi nhận thanh toán."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Orders");
        }

        // =====================================================
        // GET: /staff/reservations (Staff Table Bookings)
        // =====================================================
        [HttpGet]
        public ActionResult Reservations(string status, string date, string search)
        {
            var query = db.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .Include(r => r.Orders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(r => r.Status == status);
            }

            DateTime parsedDate;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out parsedDate))
            {
                DateTime startOfDay = parsedDate.Date;
                DateTime endOfDay = startOfDay.AddDays(1);
                query = query.Where(r => r.ReservationDate >= startOfDay && r.ReservationDate < endOfDay);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    r.User.FullName.ToLower().Contains(search) ||
                    (r.User.Phone != null && r.User.Phone.ToLower().Contains(search)) ||
                    r.Table.TableNumber.ToLower().Contains(search));
            }

            ViewBag.Tables = db.RestaurantTables.Where(t => t.IsActive).OrderBy(t => t.TableNumber).ToList();
            var reservations = query.OrderByDescending(r => r.ReservationDate).ToList();

            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(ValidReservationStatuses, status);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SelectedDate = date;
            ViewBag.SearchTerm = search;

            return View("~/Areas/Staff/Views/Staff/Reservations.cshtml", reservations);
        }

        // =====================================================
        // POST: /staff/update-reservation-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateReservationStatus(int id, string newStatus, string returnAction)
        {
            try { new RestaurantWebsite.Services.ReservationService(db).ChangeStatus(id, newStatus, (int)Session["UserId"], Convert.ToString(Session["Role"])); TempData["Success"] = "Đã cập nhật đặt bàn."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Reservations");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ChangeTable(int id, int newTableId)
        {
            try { new RestaurantWebsite.Services.ReservationService(db).ChangeTable(id, newTableId, (int)Session["UserId"], Convert.ToString(Session["Role"])); TempData["Success"] = "Đã đổi bàn."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Lịch vừa thay đổi. Vui lòng thử lại."; }
            return RedirectToAction("Reservations");
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

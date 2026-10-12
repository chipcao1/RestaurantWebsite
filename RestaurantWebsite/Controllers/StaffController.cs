using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class StaffController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly string[] ValidOrderStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivered", "Cancelled" };
        private readonly string[] ValidReservationStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };

        // =====================================================
        // GET: /staff (Dashboard)
        // =====================================================
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.CurrentRole = Session["Role"];
            ViewBag.UserName = Session["UserName"];

            DateTime todayStart = DateTime.Now.Date;
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

            return View("~/Views/Staff/Index.cshtml");
        }

        // =====================================================
        // GET: /staff/dishes (Menu Availability Management)
        // =====================================================
        [HttpGet]
        public ActionResult Dishes(string search, string categoryId)
        {
            var query = db.Dishes.Include(d => d.Category).AsQueryable();

            int catId;
            if (!string.IsNullOrWhiteSpace(categoryId) && int.TryParse(categoryId, out catId))
            {
                query = query.Where(d => d.CategoryId == catId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(search) || d.Category.Name.ToLower().Contains(search));
            }

            var dishes = query.OrderBy(d => d.Category.Name).ThenBy(d => d.Name).ToList();

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name), "Id", "Name", categoryId);
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = categoryId;

            return View("~/Views/Staff/Dishes.cshtml", dishes);
        }

        // =====================================================
        // POST: /staff/toggle-dish-status/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleDishStatus(int id)
        {
            var dish = db.Dishes.Find(id);
            if (dish == null)
            {
                TempData["Error"] = "Dish not found.";
                return RedirectToAction("Dishes");
            }

            // Toggle Status between Available and Unavailable
            dish.Status = (dish.Status == "Available") ? "Unavailable" : "Available";
            db.SaveChanges();

            TempData["Success"] = $"Dish \"{dish.Name}\" availability updated to {dish.Status}.";
            return RedirectToAction("Dishes");
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

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
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

            ViewBag.StatusList = new SelectList(ValidOrderStatuses, status);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;

            return View("~/Views/Staff/Orders.cshtml", orders);
        }

        // =====================================================
        // POST: /staff/update-order-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int id, string newStatus, string returnAction)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            if (!ValidOrderStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid order status.";
                return RedirectToAction("Orders");
            }

            order.Status = newStatus;
            db.SaveChanges();

            TempData["Success"] = $"Order #{id} status updated to \"{newStatus}\".";

            if (returnAction == "Index")
            {
                return RedirectToAction("Index");
            }
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

            var reservations = query.OrderByDescending(r => r.ReservationDate).ToList();

            ViewBag.StatusList = new SelectList(ValidReservationStatuses, status);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SelectedDate = date;
            ViewBag.SearchTerm = search;

            return View("~/Views/Staff/Reservations.cshtml", reservations);
        }

        // =====================================================
        // POST: /staff/update-reservation-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateReservationStatus(int id, string newStatus, string returnAction)
        {
            var reservation = db.Reservations.Include(r => r.Table).FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("Reservations");
            }

            if (!ValidReservationStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid reservation status.";
                return RedirectToAction("Reservations");
            }

            // Check overlap if confirming
            if (newStatus == "Confirmed")
            {
                DateTime requestedStart = reservation.ReservationDate;
                DateTime requestedEnd = requestedStart.AddHours(2);

                bool hasOverlap = db.Reservations.Any(r =>
                    r.Id != reservation.Id &&
                    r.TableId == reservation.TableId &&
                    r.Status != "Cancelled" &&
                    r.ReservationDate < requestedEnd &&
                    r.ReservationDate.AddHours(2) > requestedStart);

                if (hasOverlap)
                {
                    TempData["Error"] = $"Cannot confirm reservation #{id}. Table {reservation.Table.TableNumber} is already booked during this time.";
                    if (returnAction == "Index") return RedirectToAction("Index");
                    return RedirectToAction("Reservations");
                }
            }

            reservation.Status = newStatus;
            db.SaveChanges();

            TempData["Success"] = $"Reservation #{id} status updated to \"{newStatus}\".";

            if (returnAction == "Index")
            {
                return RedirectToAction("Index");
            }
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

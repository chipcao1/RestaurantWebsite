using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
using RestaurantWebsite.Security;

namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class ReservationsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly string[] ValidStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };

        // =====================================================
        // GET: admin/reservations (List & Filter)
        // =====================================================
        [HttpGet]
        public ActionResult Index(string status, string date, string search)
        {
            var query = db.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .Include(r => r.Orders)
                .AsQueryable();

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(r => r.Status == status);
            }

            // Filter by Date
            DateTime parsedDate;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out parsedDate))
            {
                DateTime startOfDay = parsedDate.Date;
                DateTime endOfDay = startOfDay.AddDays(1);
                query = query.Where(r => r.ReservationDate >= startOfDay && r.ReservationDate < endOfDay);
            }

            // Search by Customer Name, Email, Phone or Table Number
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    r.User.FullName.ToLower().Contains(search) ||
                    r.User.Email.ToLower().Contains(search) ||
                    (r.User.Phone != null && r.User.Phone.ToLower().Contains(search)) ||
                    r.Table.TableNumber.ToLower().Contains(search) ||
                    (r.SpecialRequest != null && r.SpecialRequest.ToLower().Contains(search)));
            }

            var reservations = query
                .OrderByDescending(r => r.ReservationDate)
                .ToList();

            ViewBag.StaffMembers = db.Users.Where(u => u.Role == "Staff" && u.IsActive).ToList();
            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(ValidStatuses, status);
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SelectedDate = date;
            ViewBag.SearchTerm = search;

            return View(reservations);
        }

        // =====================================================
        // GET: admin/reservations/details/5
        // =====================================================
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Thiếu mã đặt bàn.";
                return RedirectToAction("Index");
            }

            var reservation = db.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .Include(r => r.Orders)
                .FirstOrDefault(r => r.Id == id.Value);

            if (reservation == null)
            {
                TempData["Error"] = "Không tìm thấy đặt bàn.";
                return RedirectToAction("Index");
            }

            // Available active tables for assign/change
            ViewBag.AvailableTables = db.RestaurantTables
                .Where(t => t.IsActive)
                .OrderBy(t => t.Zone)
                .ThenBy(t => t.TableNumber)
                .ToList();

            return View(reservation);
        }

        // =====================================================
        // POST: admin/reservations/update-status
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string newStatus)
        {
            try { new RestaurantWebsite.Services.ReservationService(db).ChangeStatus(id, newStatus, (int)Session["UserId"], Convert.ToString(Session["Role"])); TempData["Success"] = "Đã cập nhật đặt bàn."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Details", new { id });
        }

        // =====================================================
        // POST: admin/reservations/change-table
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeTable(int id, int newTableId)
        {
            try { new RestaurantWebsite.Services.ReservationService(db).ChangeTable(id, newTableId, (int)Session["UserId"], Convert.ToString(Session["Role"])); TempData["Success"] = "Đã đổi bàn."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại."; }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Assign(int id, int staffId)
        {
            try { new RestaurantWebsite.Services.ReservationService(db).Assign(id, staffId); TempData["Success"] = "Đã phân công nhân viên."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Phân công vừa thay đổi. Vui lòng thử lại."; }
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

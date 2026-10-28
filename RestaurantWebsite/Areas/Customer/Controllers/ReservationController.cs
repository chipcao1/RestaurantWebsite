using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Models;
using RestaurantWebsite.Security;
using RestaurantWebsite.Filters;

namespace RestaurantWebsite.Areas.Customer.Controllers
{
    public class ReservationController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();


        // =====================================================
        // GET: Reservation/Index
        // =====================================================
        [HttpGet]
        public ActionResult Index()
        {
            var model = new Reservation
            {
                ReservationDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddHours(1),
                PartySize = 2
            };

            int? userId = null;

            if (Session["UserId"] != null)
            {
                userId = (int)Session["UserId"];
            }

            LoadReservationData(userId);

            return View("~/Areas/Customer/Views/Reservation/Index.cshtml", model);
        }


        // =====================================================
        // POST: Reservation/Book
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public ActionResult Book(
            Reservation model,
            int tableId)
        {
            try
            {
                new RestaurantWebsite.Services.ReservationService(db).Book((int)Session["UserId"], tableId, model.ReservationDate, model.PartySize, model.SpecialRequest);
                TempData["Success"] = "Đã gửi yêu cầu đặt bàn. Vui lòng chờ nhà hàng xác nhận.";
                return RedirectToRoute("CustomerReservations");
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
            catch (System.Data.DataException) { ModelState.AddModelError("", "Bàn vừa được đặt. Vui lòng chọn khung giờ khác và thử lại."); }
            LoadReservationData((int)Session["UserId"]);
            return View("~/Areas/Customer/Views/Reservation/Index.cshtml", model);
        }


        // =====================================================
        // Load data for Reservation View
        // =====================================================
        private void LoadReservationData(int? userId)
        {
            User user = null;

            if (userId.HasValue)
            {
                user = db.Users
                    .FirstOrDefault(u =>
                        u.Id == userId.Value &&
                        u.IsActive);
            }

            var availableTables = db.RestaurantTables
                .Where(t =>
                    t.IsActive &&
                    t.Status == "Available")
                .OrderBy(t => t.Capacity)
                .ThenBy(t => t.TableNumber)
                .ToList();

            ViewBag.CurrentUser = user;
            ViewBag.AvailableTables = availableTables;
        }


        // =====================================================
        // Dispose
        // =====================================================
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

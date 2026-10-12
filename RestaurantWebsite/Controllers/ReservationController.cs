using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
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
                ReservationDate = DateTime.Now.AddHours(1)
            };

            int? userId = null;

            if (Session["UserId"] != null)
            {
                userId = (int)Session["UserId"];
            }

            LoadReservationData(userId);

            return View("~/Views/Customer/Reservation/Index.cshtml", model);
        }


        // =====================================================
        // POST: Reservation/Book
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(
            Reservation model,
            int tableId)
        {
            // Must login before booking
            if (Session["UserId"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(
                            "Index",
                            "Reservation")
                    });
            }

            int userId = (int)Session["UserId"];

            var user = db.Users
                .FirstOrDefault(u =>
                    u.Id == userId &&
                    u.IsActive);

            if (user == null)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =================================================
            // Find selected table
            // =================================================
            var table = db.RestaurantTables
                .FirstOrDefault(t =>
                    t.Id == tableId &&
                    t.IsActive &&
                    t.Status == "Available");

            if (table == null)
            {
                ModelState.AddModelError(
                    "TableId",
                    "The selected table is not available.");

                LoadReservationData(userId);

                return View("~/Views/Customer/Reservation/Index.cshtml", model);
            }


            // =================================================
            // PartySize is automatically taken from table
            // =================================================
            model.TableId = table.Id;
            model.PartySize = table.Capacity;
            model.UserId = userId;

            // The PartySize field is no longer entered
            // directly by the customer in the form.
            // Remove the old validation error caused by
            // PartySize defaulting to 0 during model binding.
            ModelState.Remove("PartySize");


            // =================================================
            // Validate reservation date
            // =================================================
            if (model.ReservationDate <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "ReservationDate",
                    "Reservation date and time must be in the future.");
            }


            // =================================================
            // Validate special request
            // =================================================
            if (!string.IsNullOrWhiteSpace(model.SpecialRequest))
            {
                model.SpecialRequest = model.SpecialRequest.Trim();

                if (model.SpecialRequest.Length > 500)
                {
                    ModelState.AddModelError(
                        "SpecialRequest",
                        "Special request cannot exceed 500 characters.");
                }
            }


            // =================================================
            // Remove validation error caused by PartySize
            // =================================================
            ModelState.Remove("PartySize");


            // =================================================
            // Check reservation overlap
            // Each reservation occupies a 2-hour time window
            // =================================================
            DateTime requestedStart = model.ReservationDate;
            DateTime requestedEnd = requestedStart.AddHours(2);

            bool hasOverlap = db.Reservations.Any(r =>
                r.TableId == table.Id &&
                r.Status != "Cancelled" &&
                r.ReservationDate < requestedEnd &&
                r.ReservationDate.AddHours(2) > requestedStart);

            if (hasOverlap)
            {
                ModelState.AddModelError(
                    "TableId",
                    "This table is already reserved during the selected time.");
            }


            // =================================================
            // Reload page if validation fails
            // =================================================
            if (!ModelState.IsValid)
            {
                LoadReservationData(userId);

                return View("~/Views/Customer/Reservation/Index.cshtml", model);
            }


            // =================================================
            // Create reservation
            // =================================================
            var reservation = new Reservation
            {
                UserId = userId,
                TableId = table.Id,
                ReservationDate = model.ReservationDate,
                PartySize = table.Capacity,
                SpecialRequest = model.SpecialRequest,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            db.Reservations.Add(reservation);
            db.SaveChanges();


            // =================================================
            // Success
            // =================================================
            TempData["ReservationSuccess"] =
                "Your table has been booked successfully.";

            return RedirectToAction("Index");
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
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Models;
using RestaurantWebsite.Security;

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

            return View("~/Views/Reservation/Index.cshtml", model);
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

                return View("~/Views/Reservation/Index.cshtml", model);
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
            // Reload page if basic validation fails
            // =================================================
            if (!ModelState.IsValid)
            {
                LoadReservationData(userId);
                return View("~/Views/Reservation/Index.cshtml", model);
            }

            // =================================================
            // Atomic check & insert with Serializable transaction
            // to eliminate concurrency race condition
            // =================================================
            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    bool hasOverlap = ReservationConflictValidator.HasTimeConflict(db, table.Id, model.ReservationDate);

                    if (hasOverlap)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("TableId", "This table is already reserved during the selected time.");
                        LoadReservationData(userId);
                        return View("~/Views/Reservation/Index.cshtml", model);
                    }

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
                    transaction.Commit();

                    TempData["ReservationSuccess"] = "Your table has been booked successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Unable to complete reservation due to high demand. Please try again.");
                    LoadReservationData(userId);
                    return View("~/Views/Reservation/Index.cshtml", model);
                }
            }
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
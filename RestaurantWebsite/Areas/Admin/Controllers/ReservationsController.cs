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

            ViewBag.StatusList = new SelectList(ValidStatuses, status);
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
                TempData["Error"] = "Reservation ID was not provided.";
                return RedirectToAction("Index");
            }

            var reservation = db.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .Include(r => r.Orders)
                .FirstOrDefault(r => r.Id == id.Value);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation was not found.";
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
            var reservation = db.Reservations
                .Include(r => r.Table)
                .FirstOrDefault(r => r.Id == id);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation was not found.";
                return RedirectToAction("Index");
            }

            if (!ValidStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status specified.";
                return RedirectToAction("Details", new { id = id });
            }

            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    // If Confirming reservation, check table overlap
                    if (newStatus == "Confirmed")
                    {
                        bool hasOverlap = ReservationConflictValidator.HasTimeConflict(db, reservation.TableId, reservation.ReservationDate, reservation.Id);

                        if (hasOverlap)
                        {
                            transaction.Rollback();
                            TempData["Error"] = $"Cannot confirm reservation #{id}. Table {reservation.Table.TableNumber} is already reserved/occupied during this 2-hour window.";
                            return RedirectToAction("Details", new { id = id });
                        }
                    }

                    reservation.Status = newStatus;
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = $"Reservation #{id} status updated to \"{newStatus}\".";
                    return RedirectToAction("Details", new { id = id });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Error"] = $"Unable to update reservation #{id} due to concurrent activity. Please try again.";
                    return RedirectToAction("Details", new { id = id });
                }
            }
        }

        // =====================================================
        // POST: admin/reservations/change-table
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeTable(int id, int newTableId)
        {
            var reservation = db.Reservations
                .Include(r => r.Table)
                .FirstOrDefault(r => r.Id == id);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation was not found.";
                return RedirectToAction("Index");
            }

            var targetTable = db.RestaurantTables.FirstOrDefault(t => t.Id == newTableId && t.IsActive);
            if (targetTable == null)
            {
                TempData["Error"] = "Selected target table was not found or is inactive.";
                return RedirectToAction("Details", new { id = id });
            }

            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    bool hasOverlap = ReservationConflictValidator.HasTimeConflict(db, targetTable.Id, reservation.ReservationDate, reservation.Id);

                    if (hasOverlap)
                    {
                        transaction.Rollback();
                        TempData["Error"] = $"Cannot reassign to Table {targetTable.TableNumber}. It is already reserved during the requested time window.";
                        return RedirectToAction("Details", new { id = id });
                    }

                    string oldTableNum = reservation.Table != null ? reservation.Table.TableNumber : reservation.TableId.ToString();
                    reservation.TableId = targetTable.Id;
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = $"Reservation #{id} reassigned from Table {oldTableNum} to Table {targetTable.TableNumber}.";
                    return RedirectToAction("Details", new { id = id });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Error"] = $"Unable to reassign table for reservation #{id} due to concurrent activity. Please try again.";
                    return RedirectToAction("Details", new { id = id });
                }
            }
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

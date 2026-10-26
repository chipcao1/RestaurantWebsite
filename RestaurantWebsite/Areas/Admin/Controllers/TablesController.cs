using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class TablesController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Standard predefined zones and statuses
        private readonly string[] AvailableZones = new[] { "Indoor", "Window", "Outdoor", "VIP" };
        private readonly string[] AvailableStatuses = new[] { "Available", "Occupied", "Reserved", "Maintenance" };

        // =====================================================
        // GET: admin/tables (List & Filter)
        // =====================================================
        [HttpGet]
        public ActionResult Index(string zone, string status, string search)
        {
            var query = db.RestaurantTables
                .Include(t => t.Reservations)
                .Include(t => t.Orders)
                .AsQueryable();

            // Filter by Zone
            if (!string.IsNullOrWhiteSpace(zone) && zone != "All")
            {
                query = query.Where(t => t.Zone == zone);
            }

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(t => t.Status == status);
            }

            // Search by TableNumber or Zone
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t =>
                    t.TableNumber.ToLower().Contains(search) ||
                    t.Zone.ToLower().Contains(search));
            }

            var tables = query
                .OrderBy(t => t.Zone)
                .ThenBy(t => t.TableNumber)
                .ToList();

            ViewBag.Zones = new SelectList(AvailableZones, zone);
            ViewBag.Statuses = new SelectList(AvailableStatuses, status);
            ViewBag.SelectedZone = zone ?? "All";
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;

            return View(tables);
        }

        // =====================================================
        // GET: admin/tables/create
        // =====================================================
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Zones = new SelectList(AvailableZones, "Indoor");
            ViewBag.Statuses = new SelectList(AvailableStatuses, "Available");

            var model = new RestaurantTable
            {
                Capacity = 4,
                Zone = "Indoor",
                Status = "Available",
                IsActive = true
            };

            return View(model);
        }

        // =====================================================
        // POST: admin/tables/create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RestaurantTable model)
        {
            model.TableNumber = string.IsNullOrWhiteSpace(model.TableNumber) ? null : model.TableNumber.Trim();
            model.Zone = string.IsNullOrWhiteSpace(model.Zone) ? null : model.Zone.Trim();
            model.Status = string.IsNullOrWhiteSpace(model.Status) ? null : model.Status.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.TableNumber))
            {
                ModelState.AddModelError("TableNumber", "Table number is required.");
            }
            else if (db.RestaurantTables.Any(t => t.TableNumber.ToLower() == model.TableNumber.ToLower()))
            {
                ModelState.AddModelError("TableNumber", "A table with this number already exists.");
            }

            if (model.Capacity < 1 || model.Capacity > 50)
            {
                ModelState.AddModelError("Capacity", "Capacity must be between 1 and 50 seats.");
            }

            if (string.IsNullOrWhiteSpace(model.Zone))
            {
                ModelState.AddModelError("Zone", "Zone is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Status))
            {
                model.Status = "Available";
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Zones = new SelectList(AvailableZones, model.Zone);
                ViewBag.Statuses = new SelectList(AvailableStatuses, model.Status);
                return View(model);
            }

            db.RestaurantTables.Add(model);
            db.SaveChanges();

            TempData["Success"] = $"Table \"{model.TableNumber}\" was created successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: admin/tables/edit/5
        // =====================================================
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Table ID was not provided.";
                return RedirectToAction("Index");
            }

            var table = db.RestaurantTables
                .Include(t => t.Reservations)
                .Include(t => t.Orders)
                .FirstOrDefault(t => t.Id == id.Value);

            if (table == null)
            {
                TempData["Error"] = "Table was not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Zones = new SelectList(AvailableZones, table.Zone);
            ViewBag.Statuses = new SelectList(AvailableStatuses, table.Status);

            return View(table);
        }

        // =====================================================
        // POST: admin/tables/edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RestaurantTable model)
        {
            var table = db.RestaurantTables
                .Include(t => t.Reservations)
                .Include(t => t.Orders)
                .FirstOrDefault(t => t.Id == model.Id);

            if (table == null)
            {
                TempData["Error"] = "Table was not found.";
                return RedirectToAction("Index");
            }

            model.TableNumber = string.IsNullOrWhiteSpace(model.TableNumber) ? null : model.TableNumber.Trim();
            model.Zone = string.IsNullOrWhiteSpace(model.Zone) ? null : model.Zone.Trim();
            model.Status = string.IsNullOrWhiteSpace(model.Status) ? null : model.Status.Trim();

            if (string.IsNullOrWhiteSpace(model.TableNumber))
            {
                ModelState.AddModelError("TableNumber", "Table number is required.");
            }
            else if (db.RestaurantTables.Any(t => t.Id != model.Id && t.TableNumber.ToLower() == model.TableNumber.ToLower()))
            {
                ModelState.AddModelError("TableNumber", "Another table with this number already exists.");
            }

            if (model.Capacity < 1 || model.Capacity > 50)
            {
                ModelState.AddModelError("Capacity", "Capacity must be between 1 and 50 seats.");
            }

            if (string.IsNullOrWhiteSpace(model.Zone))
            {
                ModelState.AddModelError("Zone", "Zone is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Zones = new SelectList(AvailableZones, model.Zone);
                ViewBag.Statuses = new SelectList(AvailableStatuses, model.Status);
                model.Reservations = table.Reservations;
                model.Orders = table.Orders;
                return View(model);
            }

            table.TableNumber = model.TableNumber;
            table.Capacity = model.Capacity;
            table.Zone = model.Zone;
            table.Status = model.Status;
            table.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = $"Table \"{table.TableNumber}\" was updated successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/tables/togglestatus/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var table = db.RestaurantTables.Find(id);
            if (table == null)
            {
                TempData["Error"] = "Table was not found.";
                return RedirectToAction("Index");
            }

            table.IsActive = !table.IsActive;
            db.SaveChanges();

            string statusText = table.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"Table \"{table.TableNumber}\" has been {statusText}.";

            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/tables/delete/5 (Safe Delete)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var table = db.RestaurantTables
                .Include(t => t.Reservations)
                .Include(t => t.Orders)
                .FirstOrDefault(t => t.Id == id);

            if (table == null)
            {
                TempData["Error"] = "Table was not found.";
                return RedirectToAction("Index");
            }

            int resCount = table.Reservations.Count;
            int orderCount = table.Orders.Count;

            if (resCount > 0 || orderCount > 0)
            {
                string reason = "";
                if (resCount > 0) reason += $"{resCount} reservation(s) ";
                if (orderCount > 0) reason += $"{orderCount} order(s) ";

                TempData["Error"] = $"Cannot delete table \"{table.TableNumber}\" because it is referenced by {reason.Trim()}. Please deactivate the table or set its status to Maintenance instead of deleting.";
                return RedirectToAction("Index");
            }

            string tableNum = table.TableNumber;
            db.RestaurantTables.Remove(table);
            db.SaveChanges();

            TempData["Success"] = $"Table \"{tableNum}\" has been permanently deleted.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // DISPOSE
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

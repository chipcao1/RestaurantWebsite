using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
using RestaurantWebsite.Security;

namespace RestaurantWebsite.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminUserController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly string[] ValidRoles = new[] { "Admin", "Staff", "Customer" };

        // =====================================================
        // GET: admin/users (List & Filter)
        // =====================================================
        [HttpGet]
        public ActionResult Index(string role, string status, string search)
        {
            var query = db.Users
                .Include(u => u.Orders)
                .Include(u => u.Reservations)
                .Include(u => u.Reviews)
                .AsQueryable();

            // Filter by Role
            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                query = query.Where(u => u.Role == role);
            }

            // Filter by Active Status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                bool isActive = status == "Active";
                query = query.Where(u => u.IsActive == isActive);
            }

            // Search by Name, Email, Phone or Address
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(search)) ||
                    (u.Address != null && u.Address.ToLower().Contains(search)));
            }

            var users = query
                .OrderBy(u => u.Role)
                .ThenByDescending(u => u.CreatedAt)
                .ToList();

            ViewBag.RoleList = new SelectList(ValidRoles, role);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Inactive" }, status);
            ViewBag.SelectedRole = role ?? "All";
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;
            ViewBag.CurrentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;

            return View("~/Views/User/Index.cshtml", users);
        }

        // =====================================================
        // GET: admin/users/create-staff
        // =====================================================
        [HttpGet]
        public ActionResult CreateStaff()
        {
            var model = new User
            {
                Role = "Staff",
                IsActive = true
            };
            return View("~/Views/User/CreateStaff.cshtml", model);
        }

        // =====================================================
        // POST: admin/users/create-staff
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStaff(User model, string password)
        {
            model.FullName = string.IsNullOrWhiteSpace(model.FullName) ? null : model.FullName.Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLower();
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            model.Role = "Staff";

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (db.Users.Any(u => u.Email.ToLower() == model.Email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 6 characters long.");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/User/CreateStaff.cshtml", model);
            }

            model.PasswordHash = PasswordHasher.HashPassword(password);
            model.CreatedAt = DateTime.Now;

            db.Users.Add(model);
            db.SaveChanges();

            TempData["Success"] = $"Staff account \"{model.FullName}\" ({model.Email}) created successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: admin/users/details/5
        // =====================================================
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "User ID was not provided.";
                return RedirectToAction("Index");
            }

            var user = db.Users
                .Include(u => u.Orders.Select(o => o.Payments))
                .Include(u => u.Reservations.Select(r => r.Table))
                .Include(u => u.Reviews)
                .FirstOrDefault(u => u.Id == id.Value);

            if (user == null)
            {
                TempData["Error"] = "User account was not found.";
                return RedirectToAction("Index");
            }

            ViewBag.ValidRoles = new SelectList(ValidRoles, user.Role);
            ViewBag.CurrentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;

            return View("~/Views/User/Details.cshtml", user);
        }

        // =====================================================
        // POST: admin/users/edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User model)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
            {
                TempData["Error"] = "User account was not found.";
                return RedirectToAction("Index");
            }

            int currentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;

            model.FullName = string.IsNullOrWhiteSpace(model.FullName) ? null : model.FullName.Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLower();
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            model.Role = string.IsNullOrWhiteSpace(model.Role) ? user.Role : model.Role.Trim();

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (db.Users.Any(u => u.Id != model.Id && u.Email.ToLower() == model.Email))
            {
                ModelState.AddModelError("Email", "Another user with this email already exists.");
            }

            if (!ValidRoles.Contains(model.Role))
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
            }

            // Security check: cannot change own role or deactivate self
            if (user.Id == currentAdminId)
            {
                if (!model.IsActive)
                {
                    ModelState.AddModelError("IsActive", "You cannot deactivate your own active session account.");
                }
                if (model.Role != "Admin")
                {
                    ModelState.AddModelError("Role", "You cannot demote your own Admin account.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ValidRoles = new SelectList(ValidRoles, model.Role);
                ViewBag.CurrentAdminId = currentAdminId;
                model.Orders = user.Orders;
                model.Reservations = user.Reservations;
                model.Reviews = user.Reviews;
                return View("~/Views/User/Details.cshtml", model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            db.SaveChanges();

            // If updating currently logged in admin's own info, update Session
            if (user.Id == currentAdminId)
            {
                Session["UserName"] = user.FullName;
                Session["UserEmail"] = user.Email;
                Session["Role"] = user.Role;
            }

            TempData["Success"] = $"User \"{user.FullName}\" profile updated successfully.";
            return RedirectToAction("Details", new { id = user.Id });
        }

        // =====================================================
        // POST: admin/users/toggle-status/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "User was not found.";
                return RedirectToAction("Index");
            }

            int currentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (user.Id == currentAdminId)
            {
                TempData["Error"] = "Security Warning: You cannot deactivate your own current Admin session.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            db.SaveChanges();

            string statusText = user.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"User \"{user.FullName}\" account has been {statusText}.";

            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/users/reset-password
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, string newPassword)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "User was not found.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters long.";
                return RedirectToAction("Details", new { id = id });
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            db.SaveChanges();

            TempData["Success"] = $"Password for user \"{user.FullName}\" reset successfully.";
            return RedirectToAction("Details", new { id = id });
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

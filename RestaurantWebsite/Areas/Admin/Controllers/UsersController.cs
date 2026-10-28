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
    public class UsersController : Controller
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

            ViewBag.RoleList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(ValidRoles, role);
            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(new[] { "Active", "Inactive" }, status);
            ViewBag.SelectedRole = role ?? "All";
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;
            ViewBag.CurrentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;

            return View(users);
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
            return View(model);
        }

        // =====================================================
        // POST: admin/users/create-staff
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStaff([Bind(Include = "FullName,Email,Phone,Address,IsActive")] User model, string password)
        {
            model.FullName = string.IsNullOrWhiteSpace(model.FullName) ? null : model.FullName.Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLower();
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            model.Role = "Staff";

            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Vui lòng nhập họ tên.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
            }
            else if (db.Users.Any(u => u.Email.ToLower() == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("PasswordHash", "Vui lòng nhập mật khẩu.");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("PasswordHash", "Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.PasswordHash = PasswordHasher.HashPassword(password);
            model.CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            db.Users.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Đã tạo tài khoản nhân viên.";
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
                TempData["Error"] = "Thiếu mã người dùng.";
                return RedirectToAction("Index");
            }

            var user = db.Users
                .Include(u => u.Orders.Select(o => o.Payments))
                .Include(u => u.Reservations.Select(r => r.Table))
                .Include(u => u.Reviews)
                .FirstOrDefault(u => u.Id == id.Value);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Index");
            }

            ViewBag.CurrentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;

            return View(user);
        }

        // =====================================================
        // POST: admin/users/toggle-status/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {

                var user = db.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index");
                }

                int currentAdminId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
                if (user.Id == currentAdminId)
                {
                    TempData["Error"] = "Bạn không thể khóa tài khoản quản trị đang sử dụng.";
                    return RedirectToAction("Index");
                }

                if (user.Role == "Admin" && user.IsActive && db.Users.Count(u => u.Role == "Admin" && u.IsActive) <= 1) { TempData["Error"] = "Không thể khóa quản trị viên cuối cùng."; return RedirectToAction("Index"); }
                user.IsActive = !user.IsActive;
                db.SaveChanges(); tx.Commit();

                string statusText = user.IsActive ? "kích hoạt" : "ngừng hoạt động";
                TempData["Success"] = "Đã lưu thay đổi thành công.";

                return RedirectToAction("Index");

            }
        }

        // =====================================================
        // POST: admin/users/reset-password
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index");
            }

            user.PasswordHash = PasswordHasher.HashPassword("123456789");
            db.SaveChanges();

            TempData["Success"] = "Đã đặt lại mật khẩu tài khoản về 123456789.";
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

using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
using RestaurantWebsite.Security;

namespace RestaurantWebsite.Areas.Customer.Controllers
{
    [RoleAuthorize("Customer")]
    public class AccountController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        // =====================================================
        // CUSTOMER PROFILE
        // =====================================================

        [HttpGet]
        [RoleAuthorize("Customer")]
        public new ActionResult Profile()
        {
            var user = GetCurrentCustomer();
            if (user == null)
            {
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            return View("~/Areas/Customer/Views/Account/Profile.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public new ActionResult Profile(User model)
        {
            var user = GetCurrentCustomer();
            if (user == null)
            {
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            ModelState.Remove("IsActive");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Email");

            user.FullName = string.IsNullOrWhiteSpace(model.FullName) ? null : model.FullName.Trim();
            user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            user.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ModelState.AddModelError("FullName", "Vui lòng nhập họ tên.");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Areas/Customer/Views/Account/Profile.cshtml", user);
            }

            db.SaveChanges();
            Session["UserName"] = user.FullName;

            TempData["ProfileSuccess"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult ChangePassword()
        {
            return View("~/Areas/Customer/Views/Account/ChangePassword.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = GetCurrentCustomer();
            if (user == null)
            {
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới.");
            }
            else if (newPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu nhập lại không khớp.");
            }

            if (!string.IsNullOrWhiteSpace(currentPassword))
            {
                if (!PasswordHasher.VerifyPassword(user.PasswordHash, currentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("~/Areas/Customer/Views/Account/ChangePassword.cshtml");
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            db.SaveChanges();

            // logout the user after changing password
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Account", new { area = "" });
        }

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult MyOrders()
        {
            int userId = (int)Session["UserId"];

            var orders = db.Orders
                .Include(o => o.Reviews)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems.Select(oi => oi.Dish))
                .Include(o => o.OrderItems.Select(oi => oi.Combo))
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View("~/Areas/Customer/Views/Account/MyOrders.cshtml", orders);
        }

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult MyReservations()
        {
            int userId = (int)Session["UserId"];

            var reservations = db.Reservations
                .Include(r => r.Table)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();

            return View("~/Areas/Customer/Views/Account/MyReservations.cshtml", reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public ActionResult CancelOrder(int id)
        {
            try { new RestaurantWebsite.Services.OrderWorkflowService(db).ChangeStatus(id, "Cancelled", (int)Session["UserId"], "Customer"); TempData["Success"] = "Đã hủy thành công."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng thử lại."; }
            return RedirectToAction("MyOrders");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public ActionResult CancelReservation(int id)
        {
            try { new RestaurantWebsite.Services.ReservationService(db).ChangeStatus(id, "Cancelled", (int)Session["UserId"], "Customer"); TempData["Success"] = "Đã hủy thành công."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Dữ liệu vừa thay đổi. Vui lòng thử lại."; }
            return RedirectToAction("MyReservations");
        }

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult MyReviews()
        {
            int userId = (int)Session["UserId"];

            var reviews = db.Reviews
                .Include(r => r.Dish)
                .OrderByDescending(r => r.CreatedAt)
                .Where(r => r.UserId == userId)
                .ToList();

            return View("~/Areas/Customer/Views/Account/MyReviews.cshtml", reviews);
        }

        [HttpPost]
        [RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReview(int orderId, int dishId, int rating, string comment)
        {
            try { new RestaurantWebsite.Services.ReviewService(db).Submit((int)Session["UserId"], orderId, dishId, rating, comment); TempData["Success"] = "Đánh giá đã được gửi và đang chờ duyệt."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (System.Data.DataException) { TempData["Error"] = "Đánh giá vừa thay đổi. Vui lòng tải lại để kiểm tra."; }
            return RedirectToAction("MyReviews");
        }

        private User GetCurrentCustomer()
        {
            if (Session["UserId"] == null)
            {
                return null;
            }

            int userId = (int)Session["UserId"];

            return db.Users.FirstOrDefault(u =>
                u.Id == userId &&
                u.IsActive &&
                u.Role == "Customer");
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

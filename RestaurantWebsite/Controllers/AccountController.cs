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
    public class AccountController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // =====================================================
        // GET: Account/Login
        // =====================================================
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // =====================================================
        // POST: Account/Login
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var user = db.Users.FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower());

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            bool needsRehash;
            bool isPasswordValid = PasswordHasher.VerifyPassword(user.PasswordHash, password, out needsRehash);

            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Tự động nâng cấp hash legacy SHA-256 sang PBKDF2 an toàn khi đăng nhập thành công
            if (needsRehash)
            {
                user.PasswordHash = PasswordHasher.HashPassword(password);
                db.SaveChanges();
            }

            Session["UserId"] = user.Id;
            Session["UserName"] = user.FullName;
            Session["UserEmail"] = user.Email;
            Session["Role"] = user.Role;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (string.Equals(user.Role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToRoute("CustomerHome");
            }

            if (string.Equals(user.Role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToRoute("StaffHome");
            }

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToRoute("AdminHome");
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // =====================================================
        // GET: Account/Register
        // =====================================================
        [HttpGet]
        public ActionResult Register()
        {
            return View(new User());
        }

        // =====================================================
        // POST: Account/Register
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model, string password, string confirmPassword)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            ModelState.Remove("IsActive");
            ModelState.Remove("CreatedAt");

            model.FullName = string.IsNullOrWhiteSpace(model.FullName) ? null : model.FullName.Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
            }
            else if (db.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu nhập lại không khớp.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = PasswordHasher.HashPassword(password),
                Phone = model.Phone,
                Address = model.Address,
                Role = "Customer",
                IsActive = true,
                CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            TempData["AccountSuccess"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // =====================================================
        // GET: Account/Logout
        // =====================================================
        [HttpGet]
        public ActionResult Logout() { return View(); }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Logout")]
        public ActionResult ConfirmLogout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account", new { area = "" });
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

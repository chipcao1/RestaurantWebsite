using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

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
                ModelState.AddModelError("Email", "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Password", "Password is required.");
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
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            string hashedPassword = HashPassword(password);

            if (!string.Equals(user.PasswordHash, hashedPassword,
                               StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
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

            return RedirectToAction("Index", "Home");
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
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (db.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must contain at least 6 characters.");
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("ConfirmPassword", "Password confirmation does not match.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = HashPassword(password),
                Phone = model.Phone,
                Address = model.Address,
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            TempData["AccountSuccess"] = "Registration successful. Please log in.";
            return RedirectToAction("Login");
        }

        // =====================================================
        // GET: Account/Logout
        // =====================================================
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        // =====================================================
        // CUSTOMER PROFILE
        // =====================================================

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult Profile()
        {
            var user = GetCurrentCustomer();
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            return View("~/Views/Customer/Account/Profile.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public ActionResult Profile(User model)
        {
            var user = GetCurrentCustomer();
            if (user == null)
            {
                return RedirectToAction("Logout");
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
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Customer/Account/Profile.cshtml", user);
            }

            db.SaveChanges();
            Session["UserName"] = user.FullName;

            TempData["ProfileSuccess"] = "Personal information updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult ChangePassword()
        {
            return View("~/Views/Customer/Account/ChangePassword.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Customer")]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = GetCurrentCustomer();
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("NewPassword", "New password is required.");
            }
            else if (newPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "New password must contain at least 6 characters.");
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("ConfirmPassword", "Password confirmation does not match.");
            }

            if (!string.IsNullOrWhiteSpace(currentPassword))
            {
                string currentHash = HashPassword(currentPassword);
                if (!string.Equals(user.PasswordHash, currentHash, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Customer/Account/ChangePassword.cshtml");
            }

            user.PasswordHash = HashPassword(newPassword);
            db.SaveChanges();

            // logout the user after changing password
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }

        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult MyOrders()
        {
            int userId = (int)Session["UserId"];

            var orders = db.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Dish))
                .Include(o => o.OrderItems.Select(oi => oi.Combo))
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View("~/Views/Customer/Account/MyOrders.cshtml", orders);
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

            return View("~/Views/Customer/Account/MyReservations.cshtml", reservations);
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

            return View("~/Views/Customer/Account/MyReviews.cshtml", reviews);
        }

        [HttpPost]
        [RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReview(int orderId, int dishId, int rating, string comment)
        {
            int userId = (int)Session["UserId"];

            // Server-side rating validation
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction("MyOrders");
            }

            // Verify the order belongs to this customer
            var order = db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                TempData["Error"] = "Order not found or access denied.";
                return RedirectToAction("MyOrders");
            }

            // Verify the dish is part of this order
            bool isDishInOrder = order.OrderItems.Any(oi => oi.DishId == dishId);
            if (!isDishInOrder)
            {
                TempData["Error"] = "The specified dish is not part of this order.";
                return RedirectToAction("MyOrders");
            }

            // Check for duplicate review for this Order + Dish combination
            bool reviewExists = db.Reviews.Any(r => r.UserId == userId && r.OrderId == orderId && r.DishId == dishId);
            if (reviewExists)
            {
                TempData["Error"] = "You have already submitted a review for this dish on this order.";
                return RedirectToAction("MyOrders");
            }

            // Create and persist the review
            var review = new Review
            {
                UserId = userId,
                OrderId = orderId,
                DishId = dishId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                CreatedAt = DateTime.Now,
                IsApproved = true
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            TempData["Success"] = "Thank you! Your review has been submitted successfully.";
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                return BitConverter
                    .ToString(hash)
                    .Replace("-", "")
                    .ToLowerInvariant();
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

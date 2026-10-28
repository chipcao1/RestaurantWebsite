using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers.Management
{
    [RoleAuthorize("Staff", "Admin")]
    public abstract class DishesManagementController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public static readonly string[] ValidDishStatuses = new[]
        {
            "Available",
            "Temporarily Unavailable",
            "Discontinued"
        };

        // =====================================================
        // GET: admin/dishes (List with Filter & Search)
        // =====================================================
        [HttpGet]
        public ActionResult Index(int? categoryId, string status, string search)
        {
            var categories = db.Categories
                .OrderBy(c => c.Name)
                .ToList();

            var query = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .Include(d => d.OrderItems)
                .Include(d => d.ComboItems)
                .AsQueryable();

            // Filter by Category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (status == "Temporarily Unavailable" || status == "Unavailable")
                {
                    query = query.Where(d => d.Status == "Temporarily Unavailable" || d.Status == "Unavailable");
                }
                else
                {
                    query = query.Where(d => d.Status == status);
                }
            }

            // Search by Name or Description
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(d =>
                    d.Name.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search)));
            }

            var dishes = query
                .OrderByDescending(d => d.CreatedAt)
                .ThenBy(d => d.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;

            return View("~/Views/Management/Dishes/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", dishes);
        }

        // =====================================================
        // GET: admin/dishes/create
        // =====================================================
        [HttpGet]
        public ActionResult Create()
        {
            var categories = db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            if (!categories.Any())
            {
                // Fallback to all categories if no active ones exist
                categories = db.Categories.OrderBy(c => c.Name).ToList();
            }

            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");

            var model = new Dish
            {
                PreparationTime = 15,
                Status = "Available",
                Price = 0
            };

            return View("~/Views/Management/Dishes/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", model);
        }

        // =====================================================
        // POST: admin/dishes/create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,CategoryId,Price,PreparationTime,Status,Description")] Dish model, string primaryImageUrl)
        {
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            primaryImageUrl = string.IsNullOrWhiteSpace(primaryImageUrl) ? null : primaryImageUrl.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên món ăn.");
            }
            else if (db.Dishes.Any(d => d.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Tên món đã tồn tại.");
            }

            if (model.CategoryId <= 0 || !db.Categories.Any(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục hợp lệ.");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError("Price", "Giá không được âm.");
            }

            if (model.PreparationTime < 1 || model.PreparationTime > 300)
            {
                ModelState.AddModelError("PreparationTime", "Thời gian chế biến phải từ 1 đến 300 phút.");
            }

            if (!RestaurantWebsite.Services.CatalogRules.ValidImageUrl(primaryImageUrl)) ModelState.AddModelError("primaryImageUrl", "Đường dẫn ảnh không hợp lệ.");
            var extraImages = (Request.Form["additionalImageUrls"] ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList();
            if (extraImages.Count > 10 || extraImages.Any(x => !RestaurantWebsite.Services.CatalogRules.ValidImageUrl(x))) ModelState.AddModelError("", "Tối đa 10 ảnh bổ sung, dùng đường dẫn HTTPS hoặc ảnh nội bộ.");
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", model.CategoryId);
                ViewBag.PrimaryImageUrl = primaryImageUrl;
                return View("~/Views/Management/Dishes/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", model);
            }

            model.CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
            if (string.IsNullOrWhiteSpace(model.Status) || !ValidDishStatuses.Contains(model.Status))
            {
                if (model.Status == "Unavailable")
                {
                    model.Status = "Temporarily Unavailable";
                }
                else
                {
                    model.Status = "Available";
                }
            }

            if (!string.IsNullOrWhiteSpace(primaryImageUrl)) model.DishImages.Add(new DishImage { ImageUrl = primaryImageUrl, IsPrimary = true });
            foreach (var url in extraImages) if (!model.DishImages.Any(i => i.ImageUrl == url)) model.DishImages.Add(new DishImage { ImageUrl = url, IsPrimary = false });
            if (!model.DishImages.Any(i => i.IsPrimary) && model.DishImages.Any()) model.DishImages.First().IsPrimary = true;
            db.Dishes.Add(model); db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: admin/dishes/edit/5
        // =====================================================
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Thiếu mã món ăn.";
                return RedirectToAction("Index");
            }

            var dish = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .FirstOrDefault(d => d.Id == id.Value);

            if (dish == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", dish.CategoryId);

            var primaryImg = dish.DishImages.FirstOrDefault(i => i.IsPrimary) ?? dish.DishImages.FirstOrDefault();
            ViewBag.PrimaryImageUrl = primaryImg != null ? primaryImg.ImageUrl : string.Empty;

            return View("~/Views/Management/Dishes/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", dish);
        }

        // =====================================================
        // POST: admin/dishes/edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,CategoryId,Price,PreparationTime,Status,Description")] Dish model, string primaryImageUrl)
        {
            var dish = db.Dishes
                .Include(d => d.DishImages)
                .FirstOrDefault(d => d.Id == model.Id);

            if (dish == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn.";
                return RedirectToAction("Index");
            }

            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            primaryImageUrl = string.IsNullOrWhiteSpace(primaryImageUrl) ? null : primaryImageUrl.Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên món ăn.");
            }
            else if (db.Dishes.Any(d => d.Id != model.Id && d.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Tên món đã tồn tại.");
            }

            if (model.CategoryId <= 0 || !db.Categories.Any(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục hợp lệ.");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError("Price", "Giá không được âm.");
            }

            if (model.PreparationTime < 1 || model.PreparationTime > 300)
            {
                ModelState.AddModelError("PreparationTime", "Thời gian chế biến phải từ 1 đến 300 phút.");
            }

            if (!RestaurantWebsite.Services.CatalogRules.ValidImageUrl(primaryImageUrl)) ModelState.AddModelError("primaryImageUrl", "Đường dẫn ảnh không hợp lệ.");
            var extraImages = (Request.Form["additionalImageUrls"] ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList();
            if (extraImages.Count > 10 || extraImages.Any(x => !RestaurantWebsite.Services.CatalogRules.ValidImageUrl(x))) ModelState.AddModelError("", "Tối đa 10 ảnh bổ sung, dùng đường dẫn HTTPS hoặc ảnh nội bộ.");
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", model.CategoryId);
                ViewBag.PrimaryImageUrl = primaryImageUrl;
                model.DishImages = dish.DishImages;
                return View("~/Views/Management/Dishes/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", model);
            }

            // Update Dish properties
            dish.Name = model.Name;
            dish.CategoryId = model.CategoryId;
            dish.Price = model.Price;
            dish.PreparationTime = model.PreparationTime;
            if (ValidDishStatuses.Contains(model.Status))
            {
                dish.Status = model.Status;
            }
            else if (model.Status == "Unavailable")
            {
                dish.Status = "Temporarily Unavailable";
            }
            else
            {
                dish.Status = "Available";
            }
            dish.Description = model.Description;

            // Handle Primary DishImage
            var existingPrimary = dish.DishImages.FirstOrDefault(i => i.IsPrimary) ?? dish.DishImages.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(primaryImageUrl))
            {
                if (existingPrimary != null)
                {
                    existingPrimary.ImageUrl = primaryImageUrl;
                    existingPrimary.IsPrimary = true;
                }
                else
                {
                    db.DishImages.Add(new DishImage
                    {
                        DishId = dish.Id,
                        ImageUrl = primaryImageUrl,
                        IsPrimary = true
                    });
                }
            }
            else if (existingPrimary != null)
            {
                // Image cleared
                db.DishImages.Remove(existingPrimary);
            }

            foreach (var url in extraImages)
                if (!dish.DishImages.Any(i => i.ImageUrl == url)) db.DishImages.Add(new DishImage { DishId = dish.Id, ImageUrl = url, IsPrimary = false });
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/dishes/togglestatus/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var dish = db.Dishes.Find(id);
            if (dish == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn.";
                return RedirectToAction("Index");
            }

            dish.Status = (dish.Status == "Available") ? "Temporarily Unavailable" : "Available";
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/dishes/delete/5 (Safe Delete)
        // =====================================================
        [NonAction]
        protected ActionResult DeleteDish(int id)
        {
            var dish = db.Dishes
                .Include(d => d.DishImages)
                .Include(d => d.OrderItems)
                .Include(d => d.ComboItems)
                .Include(d => d.Reviews)
                .FirstOrDefault(d => d.Id == id);

            if (dish == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn.";
                return RedirectToAction("Index");
            }

            int orderCount = dish.OrderItems.Count;
            int comboCount = dish.ComboItems.Count;
            int reviewCount = dish.Reviews.Count;

            if (orderCount > 0 || comboCount > 0 || reviewCount > 0)
            {
                string reason = "";
                if (orderCount > 0) reason += $"{orderCount} customer order(s) ";
                if (comboCount > 0) reason += $"{comboCount} combo(s) ";
                if (reviewCount > 0) reason += $"{reviewCount} review(s) ";

                TempData["Error"] = "Dữ liệu đang được sử dụng. Hãy ngừng hoạt động thay vì xóa để giữ lịch sử.";
                return RedirectToAction("Index");
            }

            // Remove associated DishImages first
            var images = dish.DishImages.ToList();
            foreach (var img in images)
            {
                db.DishImages.Remove(img);
            }

            string dishName = dish.Name;
            db.Dishes.Remove(dish);
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // DISPOSE
        // =====================================================
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RemoveImage(int id, int imageId)
        {
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                var images = db.DishImages.Where(i => i.DishId == id).ToList(); var image = images.FirstOrDefault(i => i.Id == imageId);
                if (image == null) return HttpNotFound();
                if (image.IsPrimary) { var next = images.FirstOrDefault(i => i.Id != imageId); if (next != null) next.IsPrimary = true; }
                db.DishImages.Remove(image); db.SaveChanges(); tx.Commit();
            }
            TempData["Success"] = "Đã gỡ ảnh món ăn."; return RedirectToAction("Edit", new { id });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult PrimaryImage(int id, int imageId)
        {
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                var images = db.DishImages.Where(i => i.DishId == id).ToList();
                if (!images.Any(i => i.Id == imageId)) return HttpNotFound();
                foreach (var image in images) image.IsPrimary = image.Id == imageId;
                db.SaveChanges(); tx.Commit();
            }
            TempData["Success"] = "Đã chọn ảnh đại diện."; return RedirectToAction("Edit", new { id });
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

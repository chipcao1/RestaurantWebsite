using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers.Management
{
    [RoleAuthorize("Admin")]
    public abstract class CategoriesManagementController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // =====================================================
        // GET: admin/categories (List)
        // =====================================================
        [HttpGet]
        public ActionResult Index()
        {
            var categories = db.Categories
                .Include(c => c.Dishes)
                .OrderBy(c => c.Name)
                .ToList();

            return View("~/Views/Management/Categories/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", categories);
        }

        // =====================================================
        // GET: admin/categories/create
        // =====================================================
        [HttpGet]
        public ActionResult Create()
        {
            var model = new Category
            {
                IsActive = true
            };

            return View("~/Views/Management/Categories/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", model);
        }

        // =====================================================
        // POST: admin/categories/create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,ImageUrl,IsActive")] Category model)
        {
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên danh mục.");
            }
            else if (db.Categories.Any(c => c.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
            }

            if (!RestaurantWebsite.Services.CatalogRules.ValidImageUrl(model.ImageUrl)) ModelState.AddModelError("ImageUrl", "Đường dẫn ảnh không hợp lệ.");
            if (!ModelState.IsValid)
            {
                return View("~/Views/Management/Categories/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", model);
            }

            db.Categories.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: admin/categories/edit/5
        // =====================================================
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Thiếu mã danh mục.";
                return RedirectToAction("Index");
            }

            var category = db.Categories
                .Include(c => c.Dishes)
                .FirstOrDefault(c => c.Id == id.Value);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction("Index");
            }

            return View("~/Views/Management/Categories/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", category);
        }

        // =====================================================
        // POST: admin/categories/edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ImageUrl,IsActive")] Category model)
        {
            var category = db.Categories
                .Include(c => c.Dishes)
                .FirstOrDefault(c => c.Id == model.Id);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction("Index");
            }

            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên danh mục.");
            }
            else if (db.Categories.Any(c => c.Id != model.Id && c.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
            }

            if (!RestaurantWebsite.Services.CatalogRules.ValidImageUrl(model.ImageUrl)) ModelState.AddModelError("ImageUrl", "Đường dẫn ảnh không hợp lệ.");
            if (!ModelState.IsValid)
            {
                // Reattach dishes for view display
                model.Dishes = category.Dishes;
                return View("~/Views/Management/Categories/" + Convert.ToString(RouteData.Values["action"]) + ".cshtml", model);
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.ImageUrl = model.ImageUrl;
            category.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/categories/togglestatus/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction("Index");
            }

            category.IsActive = !category.IsActive;
            db.SaveChanges();

            string statusText = category.IsActive ? "kích hoạt" : "ngừng hoạt động";
            TempData["Success"] = "Đã lưu thay đổi thành công.";

            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/categories/delete/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.Categories
                .Include(c => c.Dishes)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction("Index");
            }

            int dishCount = category.Dishes.Count;
            if (dishCount > 0)
            {
                TempData["Error"] = "Dữ liệu đang được sử dụng. Hãy ngừng hoạt động thay vì xóa để giữ lịch sử.";
                return RedirectToAction("Index");
            }

            string categoryName = category.Name;
            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
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

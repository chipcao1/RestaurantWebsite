using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class CategoriesController : Controller
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

            return View(categories);
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

            return View(model);
        }

        // =====================================================
        // POST: admin/categories/create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category model)
        {
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Category name is required.");
            }
            else if (db.Categories.Any(c => c.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            db.Categories.Add(model);
            db.SaveChanges();

            TempData["Success"] = $"Category \"{model.Name}\" was created successfully.";
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
                TempData["Error"] = "Category ID was not provided.";
                return RedirectToAction("Index");
            }

            var category = db.Categories
                .Include(c => c.Dishes)
                .FirstOrDefault(c => c.Id == id.Value);

            if (category == null)
            {
                TempData["Error"] = "Category was not found.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // =====================================================
        // POST: admin/categories/edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category model)
        {
            var category = db.Categories
                .Include(c => c.Dishes)
                .FirstOrDefault(c => c.Id == model.Id);

            if (category == null)
            {
                TempData["Error"] = "Category was not found.";
                return RedirectToAction("Index");
            }

            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Category name is required.");
            }
            else if (db.Categories.Any(c => c.Id != model.Id && c.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Another category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                // Reattach dishes for view display
                model.Dishes = category.Dishes;
                return View(model);
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.ImageUrl = model.ImageUrl;
            category.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = $"Category \"{category.Name}\" was updated successfully.";
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
                TempData["Error"] = "Category was not found.";
                return RedirectToAction("Index");
            }

            category.IsActive = !category.IsActive;
            db.SaveChanges();

            string statusText = category.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"Category \"{category.Name}\" has been {statusText}.";

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
                TempData["Error"] = "Category was not found.";
                return RedirectToAction("Index");
            }

            int dishCount = category.Dishes.Count;
            if (dishCount > 0)
            {
                TempData["Error"] = $"Cannot delete category \"{category.Name}\" because it is currently linked to {dishCount} dish(es). To hide this category without breaking menu items, please use Deactivate instead.";
                return RedirectToAction("Index");
            }

            string categoryName = category.Name;
            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = $"Category \"{categoryName}\" has been permanently deleted.";
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

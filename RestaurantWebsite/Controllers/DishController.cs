using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class DishController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public static readonly string[] ValidDishStatuses = new[]
        {
            "Available",
            "Temporarily Unavailable",
            "Discontinued"
        };

        // =====================================================
        // GET: Dish (List with Filter & Search)
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

            return View(dishes);
        }

        // =====================================================
        // GET: Dish/Create
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

            return View(model);
        }

        // =====================================================
        // POST: Dish/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Dish model, string primaryImageUrl)
        {
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            primaryImageUrl = string.IsNullOrWhiteSpace(primaryImageUrl) ? null : primaryImageUrl.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Dish name is required.");
            }
            else if (db.Dishes.Any(d => d.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "A dish with this name already exists.");
            }

            if (model.CategoryId <= 0 || !db.Categories.Any(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category.");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError("Price", "Price cannot be negative.");
            }

            if (model.PreparationTime < 1 || model.PreparationTime > 300)
            {
                ModelState.AddModelError("PreparationTime", "Preparation time must be between 1 and 300 minutes.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", model.CategoryId);
                ViewBag.PrimaryImageUrl = primaryImageUrl;
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
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

            db.Dishes.Add(model);
            db.SaveChanges();

            // Add Primary DishImage if specified
            if (!string.IsNullOrWhiteSpace(primaryImageUrl))
            {
                var dishImage = new DishImage
                {
                    DishId = model.Id,
                    ImageUrl = primaryImageUrl,
                    IsPrimary = true
                };
                db.DishImages.Add(dishImage);
                db.SaveChanges();
            }

            TempData["Success"] = $"Dish \"{model.Name}\" was created successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: Dish/Edit/5
        // =====================================================
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Dish ID was not provided.";
                return RedirectToAction("Index");
            }

            var dish = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .FirstOrDefault(d => d.Id == id.Value);

            if (dish == null)
            {
                TempData["Error"] = "Dish was not found.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", dish.CategoryId);

            var primaryImg = dish.DishImages.FirstOrDefault(i => i.IsPrimary) ?? dish.DishImages.FirstOrDefault();
            ViewBag.PrimaryImageUrl = primaryImg != null ? primaryImg.ImageUrl : string.Empty;

            return View(dish);
        }

        // =====================================================
        // POST: Dish/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Dish model, string primaryImageUrl)
        {
            var dish = db.Dishes
                .Include(d => d.DishImages)
                .FirstOrDefault(d => d.Id == model.Id);

            if (dish == null)
            {
                TempData["Error"] = "Dish was not found.";
                return RedirectToAction("Index");
            }

            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            primaryImageUrl = string.IsNullOrWhiteSpace(primaryImageUrl) ? null : primaryImageUrl.Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Dish name is required.");
            }
            else if (db.Dishes.Any(d => d.Id != model.Id && d.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Another dish with this name already exists.");
            }

            if (model.CategoryId <= 0 || !db.Categories.Any(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category.");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError("Price", "Price cannot be negative.");
            }

            if (model.PreparationTime < 1 || model.PreparationTime > 300)
            {
                ModelState.AddModelError("PreparationTime", "Preparation time must be between 1 and 300 minutes.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", model.CategoryId);
                ViewBag.PrimaryImageUrl = primaryImageUrl;
                model.DishImages = dish.DishImages;
                return View(model);
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

            db.SaveChanges();

            TempData["Success"] = $"Dish \"{dish.Name}\" was updated successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: Dish/ToggleStatus/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var dish = db.Dishes.Find(id);
            if (dish == null)
            {
                TempData["Error"] = "Dish was not found.";
                return RedirectToAction("Index");
            }

            dish.Status = (dish.Status == "Available") ? "Temporarily Unavailable" : "Available";
            db.SaveChanges();

            TempData["Success"] = $"Status of dish \"{dish.Name}\" changed to \"{dish.Status}\".";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: Dish/Delete/5 (Safe Delete)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var dish = db.Dishes
                .Include(d => d.DishImages)
                .Include(d => d.OrderItems)
                .Include(d => d.ComboItems)
                .Include(d => d.Reviews)
                .FirstOrDefault(d => d.Id == id);

            if (dish == null)
            {
                TempData["Error"] = "Dish was not found.";
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

                TempData["Error"] = $"Cannot delete dish \"{dish.Name}\" because it is linked to {reason.Trim()}. Please set its status to \"Temporarily Unavailable\" or \"Discontinued\" instead of deleting.";
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

            TempData["Success"] = $"Dish \"{dishName}\" has been permanently deleted.";
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

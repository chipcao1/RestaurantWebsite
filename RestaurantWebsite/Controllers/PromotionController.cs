using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
{
    [RoleAuthorize("Admin")]
    public class PromotionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly string[] DiscountTypes = new[] { "Percentage", "FixedAmount" };

        // =====================================================
        // GET: Promotion (List & Filter)
        // =====================================================
        [HttpGet]
        public ActionResult Index(string discountType, string status, string search)
        {
            var query = db.Promotions
                .Include(p => p.OrderPromotions)
                .AsQueryable();

            // Filter by DiscountType
            if (!string.IsNullOrWhiteSpace(discountType) && discountType != "All")
            {
                query = query.Where(p => p.DiscountType == discountType);
            }

            // Filter by Status (Active, Inactive, Expired)
            DateTime now = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (status == "Active")
                {
                    query = query.Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now);
                }
                else if (status == "Inactive")
                {
                    query = query.Where(p => !p.IsActive);
                }
                else if (status == "Expired")
                {
                    query = query.Where(p => p.EndDate < now);
                }
                else if (status == "Upcoming")
                {
                    query = query.Where(p => p.StartDate > now);
                }
            }

            // Search by Code or Name
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Code.ToLower().Contains(search) ||
                    p.Name.ToLower().Contains(search) ||
                    (p.Description != null && p.Description.ToLower().Contains(search)));
            }

            var promotions = query
                .OrderByDescending(p => p.Id)
                .ToList();

            ViewBag.DiscountTypes = new SelectList(DiscountTypes, discountType);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Inactive", "Expired", "Upcoming" }, status);
            ViewBag.SelectedDiscountType = discountType ?? "All";
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;

            return View(promotions);
        }

        // =====================================================
        // GET: Promotion/Create
        // =====================================================
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.DiscountTypes = new SelectList(DiscountTypes, "Percentage");

            var model = new Promotion
            {
                DiscountType = "Percentage",
                DiscountValue = 10,
                MinimumOrderAmount = 0,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                IsActive = true
            };

            return View(model);
        }

        // =====================================================
        // POST: Promotion/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Promotion model)
        {
            model.Code = string.IsNullOrWhiteSpace(model.Code) ? null : model.Code.Trim().ToUpper();
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.DiscountType = string.IsNullOrWhiteSpace(model.DiscountType) ? "Percentage" : model.DiscountType.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Promotion code is required.");
            }
            else if (db.Promotions.Any(p => p.Code.ToUpper() == model.Code))
            {
                ModelState.AddModelError("Code", "A promotion code with this value already exists.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Promotion name is required.");
            }

            if (model.DiscountValue <= 0)
            {
                ModelState.AddModelError("DiscountValue", "Discount value must be greater than 0.");
            }

            if (model.DiscountType == "Percentage" && model.DiscountValue > 100)
            {
                ModelState.AddModelError("DiscountValue", "Percentage discount cannot exceed 100%.");
            }

            if (model.MinimumOrderAmount < 0)
            {
                ModelState.AddModelError("MinimumOrderAmount", "Minimum order amount cannot be negative.");
            }

            if (model.MaximumDiscount.HasValue && model.MaximumDiscount.Value < 0)
            {
                ModelState.AddModelError("MaximumDiscount", "Maximum discount cannot be negative.");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be later than start date.");
            }

            if (model.UsageLimit.HasValue && model.UsageLimit.Value < 1)
            {
                ModelState.AddModelError("UsageLimit", "Usage limit must be at least 1 if set.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DiscountTypes = new SelectList(DiscountTypes, model.DiscountType);
                return View(model);
            }

            db.Promotions.Add(model);
            db.SaveChanges();

            TempData["Success"] = $"Promotion \"{model.Code}\" ({model.Name}) created successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: Promotion/Edit/5
        // =====================================================
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Promotion ID was not provided.";
                return RedirectToAction("Index");
            }

            var promotion = db.Promotions
                .Include(p => p.OrderPromotions)
                .FirstOrDefault(p => p.Id == id.Value);

            if (promotion == null)
            {
                TempData["Error"] = "Promotion was not found.";
                return RedirectToAction("Index");
            }

            ViewBag.DiscountTypes = new SelectList(DiscountTypes, promotion.DiscountType);
            return View(promotion);
        }

        // =====================================================
        // POST: Promotion/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Promotion model)
        {
            var promotion = db.Promotions
                .Include(p => p.OrderPromotions)
                .FirstOrDefault(p => p.Id == model.Id);

            if (promotion == null)
            {
                TempData["Error"] = "Promotion was not found.";
                return RedirectToAction("Index");
            }

            model.Code = string.IsNullOrWhiteSpace(model.Code) ? null : model.Code.Trim().ToUpper();
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.DiscountType = string.IsNullOrWhiteSpace(model.DiscountType) ? "Percentage" : model.DiscountType.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Promotion code is required.");
            }
            else if (db.Promotions.Any(p => p.Id != model.Id && p.Code.ToUpper() == model.Code))
            {
                ModelState.AddModelError("Code", "Another promotion code with this value already exists.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Promotion name is required.");
            }

            if (model.DiscountValue <= 0)
            {
                ModelState.AddModelError("DiscountValue", "Discount value must be greater than 0.");
            }

            if (model.DiscountType == "Percentage" && model.DiscountValue > 100)
            {
                ModelState.AddModelError("DiscountValue", "Percentage discount cannot exceed 100%.");
            }

            if (model.MinimumOrderAmount < 0)
            {
                ModelState.AddModelError("MinimumOrderAmount", "Minimum order amount cannot be negative.");
            }

            if (model.MaximumDiscount.HasValue && model.MaximumDiscount.Value < 0)
            {
                ModelState.AddModelError("MaximumDiscount", "Maximum discount cannot be negative.");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be later than start date.");
            }

            if (model.UsageLimit.HasValue && model.UsageLimit.Value < 1)
            {
                ModelState.AddModelError("UsageLimit", "Usage limit must be at least 1 if set.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DiscountTypes = new SelectList(DiscountTypes, model.DiscountType);
                model.OrderPromotions = promotion.OrderPromotions;
                model.UsedCount = promotion.UsedCount;
                return View(model);
            }

            promotion.Code = model.Code;
            promotion.Name = model.Name;
            promotion.Description = model.Description;
            promotion.DiscountType = model.DiscountType;
            promotion.DiscountValue = model.DiscountValue;
            promotion.MinimumOrderAmount = model.MinimumOrderAmount;
            promotion.MaximumDiscount = model.MaximumDiscount;
            promotion.StartDate = model.StartDate;
            promotion.EndDate = model.EndDate;
            promotion.UsageLimit = model.UsageLimit;
            promotion.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = $"Promotion \"{promotion.Code}\" updated successfully.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: Promotion/ToggleStatus/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var promotion = db.Promotions.Find(id);
            if (promotion == null)
            {
                TempData["Error"] = "Promotion was not found.";
                return RedirectToAction("Index");
            }

            promotion.IsActive = !promotion.IsActive;
            db.SaveChanges();

            string statusText = promotion.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"Promotion \"{promotion.Code}\" has been {statusText}.";

            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: Promotion/Delete/5 (Safe Delete)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var promotion = db.Promotions
                .Include(p => p.OrderPromotions)
                .FirstOrDefault(p => p.Id == id);

            if (promotion == null)
            {
                TempData["Error"] = "Promotion was not found.";
                return RedirectToAction("Index");
            }

            int linkedOrdersCount = promotion.OrderPromotions.Count;
            if (linkedOrdersCount > 0 || promotion.UsedCount > 0)
            {
                TempData["Error"] = $"Cannot delete promotion \"{promotion.Code}\" because it has been used in {linkedOrdersCount} order(s) (UsedCount: {promotion.UsedCount}). You should deactivate it instead to preserve sales history.";
                return RedirectToAction("Index");
            }

            string code = promotion.Code;
            db.Promotions.Remove(promotion);
            db.SaveChanges();

            TempData["Success"] = $"Promotion \"{code}\" has been permanently deleted.";
            return RedirectToAction("Index");
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

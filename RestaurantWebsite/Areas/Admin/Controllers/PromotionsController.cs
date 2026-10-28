using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class PromotionsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly string[] DiscountTypes = new[] { "Percentage", "FixedAmount" };

        // =====================================================
        // GET: admin/promotions (List & Filter)
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
            DateTime now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
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

            ViewBag.DiscountTypes = RestaurantWebsite.Infrastructure.ViewOptions.Choices(DiscountTypes, discountType);
            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(new[] { "Active", "Inactive", "Expired", "Upcoming" }, status);
            ViewBag.SelectedDiscountType = discountType ?? "All";
            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.SearchTerm = search;

            return View(promotions);
        }

        // =====================================================
        // GET: admin/promotions/create
        // =====================================================
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.DiscountTypes = RestaurantWebsite.Infrastructure.ViewOptions.Choices(DiscountTypes, "Percentage");

            var model = new Promotion
            {
                DiscountType = "Percentage",
                DiscountValue = 10,
                MinimumOrderAmount = 0,
                StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now,
                EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(30),
                IsActive = true
            };

            return View(model);
        }

        // =====================================================
        // POST: admin/promotions/create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Description,DiscountType,DiscountValue,MinimumOrderAmount,MaximumDiscount,StartDate,EndDate,UsageLimit,IsActive")] Promotion model)
        {
            model.Code = string.IsNullOrWhiteSpace(model.Code) ? null : model.Code.Trim().ToUpper();
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.DiscountType = string.IsNullOrWhiteSpace(model.DiscountType) ? "Percentage" : model.DiscountType.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Vui lòng nhập mã khuyến mãi.");
            }
            else if (db.Promotions.Any(p => p.Code.ToUpper() == model.Code))
            {
                ModelState.AddModelError("Code", "Mã khuyến mãi đã tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên khuyến mãi.");
            }

            if (model.DiscountValue <= 0)
            {
                ModelState.AddModelError("DiscountValue", "Mức giảm phải lớn hơn 0.");
            }

            if (model.DiscountType == "Percentage" && model.DiscountValue > 100)
            {
                ModelState.AddModelError("DiscountValue", "Mức giảm phần trăm không được vượt quá 100%.");
            }

            if (model.MinimumOrderAmount < 0)
            {
                ModelState.AddModelError("MinimumOrderAmount", "Giá trị đơn tối thiểu không được âm.");
            }

            if (model.MaximumDiscount.HasValue && model.MaximumDiscount.Value < 0)
            {
                ModelState.AddModelError("MaximumDiscount", "Mức giảm tối đa không được âm.");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            if (model.UsageLimit.HasValue && model.UsageLimit.Value < 1)
            {
                ModelState.AddModelError("UsageLimit", "Giới hạn sử dụng cần ít nhất 1 lượt nếu có nhập.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DiscountTypes = RestaurantWebsite.Infrastructure.ViewOptions.Choices(DiscountTypes, model.DiscountType);
                return View(model);
            }

            db.Promotions.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // GET: admin/promotions/edit/5
        // =====================================================
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Error"] = "Thiếu mã khuyến mãi.";
                return RedirectToAction("Index");
            }

            var promotion = db.Promotions
                .Include(p => p.OrderPromotions)
                .FirstOrDefault(p => p.Id == id.Value);

            if (promotion == null)
            {
                TempData["Error"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction("Index");
            }

            ViewBag.DiscountTypes = RestaurantWebsite.Infrastructure.ViewOptions.Choices(DiscountTypes, promotion.DiscountType);
            return View(promotion);
        }

        // =====================================================
        // POST: admin/promotions/edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Description,DiscountType,DiscountValue,MinimumOrderAmount,MaximumDiscount,StartDate,EndDate,UsageLimit,IsActive")] Promotion model)
        {
            var promotion = db.Promotions
                .Include(p => p.OrderPromotions)
                .FirstOrDefault(p => p.Id == model.Id);

            if (promotion == null)
            {
                TempData["Error"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction("Index");
            }

            model.Code = string.IsNullOrWhiteSpace(model.Code) ? null : model.Code.Trim().ToUpper();
            model.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.DiscountType = string.IsNullOrWhiteSpace(model.DiscountType) ? "Percentage" : model.DiscountType.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Vui lòng nhập mã khuyến mãi.");
            }
            else if (db.Promotions.Any(p => p.Id != model.Id && p.Code.ToUpper() == model.Code))
            {
                ModelState.AddModelError("Code", "Mã khuyến mãi đã tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên khuyến mãi.");
            }

            if (model.DiscountValue <= 0)
            {
                ModelState.AddModelError("DiscountValue", "Mức giảm phải lớn hơn 0.");
            }

            if (model.DiscountType == "Percentage" && model.DiscountValue > 100)
            {
                ModelState.AddModelError("DiscountValue", "Mức giảm phần trăm không được vượt quá 100%.");
            }

            if (model.MinimumOrderAmount < 0)
            {
                ModelState.AddModelError("MinimumOrderAmount", "Giá trị đơn tối thiểu không được âm.");
            }

            if (model.MaximumDiscount.HasValue && model.MaximumDiscount.Value < 0)
            {
                ModelState.AddModelError("MaximumDiscount", "Mức giảm tối đa không được âm.");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            if (model.UsageLimit.HasValue && model.UsageLimit.Value < 1)
            {
                ModelState.AddModelError("UsageLimit", "Giới hạn sử dụng cần ít nhất 1 lượt nếu có nhập.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DiscountTypes = RestaurantWebsite.Infrastructure.ViewOptions.Choices(DiscountTypes, model.DiscountType);
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

            TempData["Success"] = "Đã lưu thay đổi thành công.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/promotions/togglestatus/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var promotion = db.Promotions.Find(id);
            if (promotion == null)
            {
                TempData["Error"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction("Index");
            }

            promotion.IsActive = !promotion.IsActive;
            db.SaveChanges();

            string statusText = promotion.IsActive ? "kích hoạt" : "ngừng hoạt động";
            TempData["Success"] = "Đã lưu thay đổi thành công.";

            return RedirectToAction("Index");
        }

        // =====================================================
        // POST: admin/promotions/delete/5 (Safe Delete)
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
                TempData["Error"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction("Index");
            }

            int linkedOrdersCount = promotion.OrderPromotions.Count;
            if (linkedOrdersCount > 0 || promotion.UsedCount > 0)
            {
                TempData["Error"] = "Dữ liệu đang được sử dụng. Hãy ngừng hoạt động thay vì xóa để giữ lịch sử.";
                return RedirectToAction("Index");
            }

            string code = promotion.Code;
            db.Promotions.Remove(promotion);
            db.SaveChanges();

            TempData["Success"] = "Đã lưu thay đổi thành công.";
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

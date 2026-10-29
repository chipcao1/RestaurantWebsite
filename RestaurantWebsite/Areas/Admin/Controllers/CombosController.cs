using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class CombosController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        public ActionResult Index(string search, string status)
        {
            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
            var combos = db.Combos.Include(c => c.ComboItems.Select(i => i.Dish.Category)).Include(c => c.ComboItems.Select(i => i.Dish.DishImages)).OrderBy(c => c.Name).ToList();
            ViewBag.TotalCombos = combos.Count;
            ViewBag.AvailableCombos = combos.Count(c => RestaurantWebsite.Services.CatalogRules.ComboStatus(c, now) == "Active");
            ViewBag.InactiveCombos = combos.Count(c => !c.IsActive);
            ViewBag.Search = (search ?? "").Trim();
            ViewBag.Status = status;
            ViewBag.StatusList = RestaurantWebsite.Infrastructure.ViewOptions.Choices(new[] { "Active", "Inactive", "Upcoming", "Expired", "Unavailable" }, status);
            if (!string.IsNullOrWhiteSpace(search))
                combos = combos.Where(c => (c.Name + " " + c.Description).IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (!string.IsNullOrWhiteSpace(status))
                combos = combos.Where(c => RestaurantWebsite.Services.CatalogRules.ComboStatus(c, now) == status).ToList();
            return View(combos);
        }
        public ActionResult Edit(int? id)
        {
            var model = id.HasValue ? db.Combos.Include(c => c.ComboItems.Select(i => i.Dish.DishImages)).SingleOrDefault(c => c.Id == id) : new Combo();
            if (model == null) return HttpNotFound();
            LoadDishes(); return View(model);
        }
        private void LoadDishes() { ViewBag.Dishes = db.Dishes.Include(d => d.Category).Where(d => d.Status != "Discontinued").OrderBy(d => d.Name).ToList(); }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Price,ImageUrl,IsActive,StartDate,EndDate")] Combo model, int[] dishIds, int[] quantities)
        {
            dishIds = dishIds ?? new int[0]; quantities = quantities ?? new int[0];
            if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError("Name", "Vui lòng nhập tên suất ăn.");
            if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate) ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
            if (!RestaurantWebsite.Services.CatalogRules.ValidImageUrl(model.ImageUrl)) ModelState.AddModelError("ImageUrl", "Ảnh cần đường dẫn HTTPS hoặc đường dẫn trong website.");
            if (dishIds.Length == 0 || dishIds.Length != quantities.Length || dishIds.Distinct().Count() != dishIds.Length || quantities.Any(q => q < 1 || q > 100)) ModelState.AddModelError("", "Chọn ít nhất một món, không trùng món, số lượng từ 1 đến 100.");
            if (!ModelState.IsValid) { LoadDishes(); return View(model); }
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (db.Dishes.Count(d => dishIds.Contains(d.Id) && d.Status != "Discontinued") != dishIds.Length) { ModelState.AddModelError("", "Có món đã ngừng kinh doanh."); LoadDishes(); return View(model); }
                var combo = model.Id == 0 ? new Combo() : db.Combos.Include(c => c.ComboItems).SingleOrDefault(c => c.Id == model.Id);
                if (combo == null) return HttpNotFound();
                combo.Name = model.Name.Trim(); combo.Description = model.Description; combo.Price = model.Price; combo.ImageUrl = model.ImageUrl; combo.IsActive = model.IsActive; combo.StartDate = model.StartDate; combo.EndDate = model.EndDate;
                if (model.Id == 0) db.Combos.Add(combo);
                foreach (var i in combo.ComboItems.ToList()) db.ComboItems.Remove(i);
                for (int i = 0; i < dishIds.Length; i++) combo.ComboItems.Add(new ComboItem { DishId = dishIds[i], Quantity = quantities[i] });
                db.SaveChanges(); tx.Commit();
            }
            TempData["Success"] = "Đã lưu suất ăn kết hợp."; return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var combo = db.Combos.Find(id); if (combo == null) return HttpNotFound();
            combo.IsActive = !combo.IsActive; db.SaveChanges(); TempData["Success"] = "Đã đổi trạng thái suất ăn."; return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}

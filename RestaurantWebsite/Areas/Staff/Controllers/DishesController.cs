using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
using RestaurantWebsite.Services;

namespace RestaurantWebsite.Areas.Staff.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class DishesController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpGet]
        public ActionResult Index(int? categoryId, string search)
        {
            var query = db.Dishes.Include(d => d.Category).Include(d => d.DishImages).AsQueryable();
            if (categoryId.HasValue) query = query.Where(d => d.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                var term = search.ToLowerInvariant();
                query = query.Where(d => d.Name.ToLower().Contains(term) || d.Category.Name.ToLower().Contains(term));
            }
            ViewBag.Categories = new SelectList(db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", categoryId);
            ViewBag.SelectedCategory = categoryId.HasValue ? categoryId.Value.ToString() : null;
            ViewBag.SearchTerm = search;
            return View("~/Areas/Staff/Views/Staff/Dishes.cshtml", query.OrderBy(d => d.Category.Name).ThenBy(d => d.Name).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id, int? categoryId, string search)
        {
            try
            {
                new DishAvailabilityService(db).Toggle(id);
                TempData["Success"] = "Đã cập nhật tình trạng phục vụ món ăn.";
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index", new { categoryId, search });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

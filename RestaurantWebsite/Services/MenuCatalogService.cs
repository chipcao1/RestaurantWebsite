using System.Data.Entity;
using System.Linq;
using RestaurantWebsite.Infrastructure;
using RestaurantWebsite.Models;
using RestaurantWebsite.ViewModels;

namespace RestaurantWebsite.Services
{
    public sealed class MenuCatalogService
    {
        private readonly AppDbContext db;
        public MenuCatalogService(AppDbContext context) { db = context; }

        public MenuCatalogViewModel Load(string search = null, int? categoryId = null)
        {
            var now = RestaurantClock.Now;
            var categories = db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            // Keep the complete dataset: category/search only set initial visibility.
            // Switching back to All must not require another page or lose dishes.
            return new MenuCatalogViewModel
            {
                Categories = categories,
                Dishes = db.Dishes.AsNoTracking().Include(d => d.Category).Include(d => d.DishImages)
                    .Where(d => d.Status == "Available" && d.Category.IsActive).OrderBy(d => d.Name).ToList(),
                Combos = db.Combos.AsNoTracking().Include(c => c.ComboItems.Select(i => i.Dish.Category))
                    .Where(c => c.IsActive && (!c.StartDate.HasValue || c.StartDate <= now) && (!c.EndDate.HasValue || c.EndDate >= now))
                    .OrderBy(c => c.Name).ToList().Where(c => CatalogRules.Purchasable(c, now)).ToList(),
                Search = (search ?? "").Trim(),
                SelectedCategoryId = categories.Any(c => c.Id == categoryId) ? categoryId : null
            };
        }
    }
}

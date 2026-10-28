using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Customer.Controllers
{
    [RoleAuthorize("Customer")]
    public class CustomerController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /customer
        [HttpGet]
        public ActionResult Index()
        {
            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            var categories = db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            var dishes = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .Where(d => d.Status == "Available" && d.Category.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .ThenBy(d => d.Name)
                .Take(6)
                .ToList();

            var promotion = db.Promotions
                .Where(p => p.IsActive &&
                            p.StartDate <= now &&
                            p.EndDate >= now)
                .OrderBy(p => p.EndDate)
                .FirstOrDefault();

            var combo = db.Combos
                .Where(c => c.IsActive &&
                            (!c.StartDate.HasValue || c.StartDate.Value <= now) &&
                            (!c.EndDate.HasValue || c.EndDate.Value >= now))
                .OrderBy(c => c.EndDate)
                .FirstOrDefault();

            ViewBag.HomeCategories = categories;
            ViewBag.HomeDishes = dishes;
            ViewBag.HomePromotion = promotion;
            ViewBag.HomeCombo = combo;

            return View("~/Views/Home/Index.cshtml");
        }

        // GET: /customer/about
        [HttpGet]
        public ActionResult About()
        {
            return View("~/Views/Home/About.cshtml");
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

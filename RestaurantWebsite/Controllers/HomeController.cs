using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // =====================================================
        // HOME
        // =====================================================
        public ActionResult Index()
        {
            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            // -------------------------------------------------
            // ACTIVE CATEGORIES
            // -------------------------------------------------
            var categories = db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();


            // -------------------------------------------------
            // AVAILABLE DISHES
            // Hiển thị 6 món trên Home
            // -------------------------------------------------
            var dishes = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .Where(d => d.Status == "Available" && d.Category.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .ThenBy(d => d.Name)
                .Take(6)
                .ToList();


            // -------------------------------------------------
            // ACTIVE PROMOTION
            // Lấy promotion đang còn hiệu lực
            // -------------------------------------------------
            var promotion = db.Promotions
                .Where(p =>
                    p.IsActive &&
                    p.StartDate <= now &&
                    p.EndDate >= now)
                .OrderBy(p => p.EndDate)
                .FirstOrDefault();


            // -------------------------------------------------
            // ACTIVE COMBO
            // Lấy combo đang còn hiệu lực
            // -------------------------------------------------
            var combo = db.Combos
                .Where(c =>
                    c.IsActive &&
                    (!c.StartDate.HasValue || c.StartDate.Value <= now) &&
                    (!c.EndDate.HasValue || c.EndDate.Value >= now))
                .OrderBy(c => c.EndDate)
                .FirstOrDefault();


            // -------------------------------------------------
            // SEND DATA TO VIEW
            // -------------------------------------------------
            ViewBag.HomeCategories = categories;
            ViewBag.HomeDishes = dishes;
            ViewBag.HomePromotion = promotion;
            ViewBag.HomeCombo = combo;


            return View();
        }


        // =====================================================
        // ABOUT
        // =====================================================
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }


        // =====================================================
        // CONTACT
        // =====================================================
        public ActionResult Contact()
        {
            return RedirectToAction("About");
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

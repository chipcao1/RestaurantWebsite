using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Customer.Controllers
{
    public class ComboController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();




        // =====================================================
        // GET: Combo/Details/5
        // =====================================================
        [HttpGet]
        public ActionResult Index() { return RedirectToAction("Index", "Menu", new { area = "Customer" }); }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound("Thiếu mã suất ăn.");
            }

            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            var combo = db.Combos
                .Include(c => c.ComboItems.Select(ci => ci.Dish))
                .FirstOrDefault(c =>
                    c.Id == id.Value &&
                    c.IsActive &&
                    (!c.StartDate.HasValue || c.StartDate.Value <= now) &&
                    (!c.EndDate.HasValue || c.EndDate.Value >= now));

            if (combo == null)
            {
                return HttpNotFound("Không tìm thấy suất ăn.");
            }

            return PartialView("~/Areas/Customer/Views/Combo/Details.cshtml", combo);
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

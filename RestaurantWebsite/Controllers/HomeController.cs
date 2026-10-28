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
        public ActionResult Index(string search, int? categoryId)
        {
            return View(new RestaurantWebsite.Services.MenuCatalogService(db).Load(search, categoryId));
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

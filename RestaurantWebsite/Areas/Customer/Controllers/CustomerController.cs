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
        public ActionResult Index(string search, int? categoryId)
        {
            return View("~/Views/Home/Index.cshtml", new RestaurantWebsite.Services.MenuCatalogService(db).Load(search, categoryId));
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

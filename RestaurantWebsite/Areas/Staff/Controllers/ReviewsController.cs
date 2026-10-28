using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
namespace RestaurantWebsite.Areas.Staff.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        public ActionResult Index() { return View("~/Areas/Admin/Views/Reviews/Index.cshtml", db.Reviews.Include(r => r.Dish).Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToList()); }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}

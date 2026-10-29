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
        public ActionResult Index(bool? approved, string search)
        {
            var q = db.Reviews.Include(r => r.Dish).Include(r => r.User).AsQueryable();
            if (approved.HasValue) q = q.Where(r => r.IsApproved == approved.Value);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(r => r.Comment.Contains(search) || r.User.FullName.Contains(search) || r.Dish.Name.Contains(search));
            ViewBag.Search = search;
            ViewBag.Approved = approved.HasValue ? approved.Value.ToString().ToLowerInvariant() : "";
            return View("~/Areas/Admin/Views/Reviews/Index.cshtml", q.OrderByDescending(r => r.CreatedAt).ToList());
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}

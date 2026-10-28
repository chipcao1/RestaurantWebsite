using System.Web.Mvc;
using RestaurantWebsite.Models;
using RestaurantWebsite.Services;
using RestaurantWebsite.Filters;
namespace RestaurantWebsite.Areas.Staff.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class ReportsController : Controller
    {
        public ActionResult Menu() { using (var db = new AppDbContext()) return File(PdfService.Create("THỰC ĐƠN NHÀ HÀNG", new ReportService(db).Menu()), "application/pdf", "thuc-don.pdf"); }
    }
}

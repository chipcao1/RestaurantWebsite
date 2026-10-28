using System;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;
using RestaurantWebsite.Services;
using RestaurantWebsite.Infrastructure;
namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        public ActionResult Index(DateTime? from, DateTime? to, bool pdf = false)
        {
            try
            {
                var report = new ReportService(db).Sales(from ?? new DateTime(RestaurantClock.Now.Year, RestaurantClock.Now.Month, 1), to ?? RestaurantClock.Now.Date);
                if (pdf) return File(PdfService.Create("BÁO CÁO DOANH THU", new[] { "Từ " + report.From.ToString("dd/MM/yyyy") + " đến " + report.To.ToString("dd/MM/yyyy"), "Khoản đã hoàn được điều chỉnh theo ngày thanh toán gốc; không có ngày hoàn riêng.", "Tổng thu: " + Vietnamese.Money(report.Collected), "Đã hoàn: " + Vietnamese.Money(report.Refunded), "Thu ròng: " + Vietnamese.Money(report.Net) }.ConcatReport(report.Lines)), "application/pdf", "doanh-thu.pdf");
                return View(report);
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Index", new { from = RestaurantClock.Now.Date.ToString("yyyy-MM-dd"), to = RestaurantClock.Now.Date.ToString("yyyy-MM-dd") }); }
        }
        public ActionResult Menu() { return File(PdfService.Create("BÁO CÁO THỰC ĐƠN", new ReportService(db).Menu()), "application/pdf", "thuc-don.pdf"); }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
    internal static class ReportLines
    { public static System.Collections.Generic.IEnumerable<string> ConcatReport(this System.Collections.Generic.IEnumerable<string> a, System.Collections.Generic.IEnumerable<string> b) { return System.Linq.Enumerable.Concat(a, b); } }
}

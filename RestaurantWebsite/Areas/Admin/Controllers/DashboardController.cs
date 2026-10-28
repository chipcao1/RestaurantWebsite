using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
            var todayStart = now.Date;
            var tomorrowStart = todayStart.AddDays(1);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var validOrdersQuery = db.Orders.Where(o => o.Status != "Cancelled");

            // 1. KPI Today
            var todayValidOrders = validOrdersQuery
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < tomorrowStart);

            ViewBag.TodayRevenue = new RestaurantWebsite.Services.ReportService(db).Sales(todayStart, todayStart).Net;
            ViewBag.TodayOrderCount = todayValidOrders.Count();

            // 2. KPI Month
            var monthValidOrdersQuery = validOrdersQuery
                .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < nextMonthStart);

            ViewBag.MonthRevenue = new RestaurantWebsite.Services.ReportService(db).Sales(monthStart, nextMonthStart.AddDays(-1)).Net;
            ViewBag.MonthOrderCount = monthValidOrdersQuery.Count();

            // 3. Operational Counts
            ViewBag.PendingOrdersCount = db.Orders.Count(o => o.Status == "Pending");
            ViewBag.TodayReservationsCount = db.Reservations
                .Count(r => r.ReservationDate >= todayStart && r.ReservationDate < tomorrowStart);
            ViewBag.TotalCustomersCount = db.Users.Count(u => u.Role == "Customer");
            ViewBag.ActivePromotionsCount = db.Promotions
                .Count(p => p.IsActive && p.StartDate <= now && p.EndDate >= now);

            var monthValidOrders = validOrdersQuery
                .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < nextMonthStart)
                .Select(o => new { o.Id, o.CreatedAt, o.TotalAmount, o.OrderType })
                .ToList();

            // 4 & 8. Hourly peak distribution & Peak business hour
            var hourlyDistribution = monthValidOrders
                .GroupBy(o => o.CreatedAt.Hour)
                .Select(g => new PeakHourItem
                {
                    Hour = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Hour)
                .ToList();

            var peakHourObj = hourlyDistribution.OrderByDescending(x => x.OrderCount).FirstOrDefault();
            string peakBusinessHour = peakHourObj != null
                ? string.Format("{0:D2}:00 - {1:D2}:00 ({2} orders)", peakHourObj.Hour, peakHourObj.Hour + 1, peakHourObj.OrderCount)
                : "N/A";
            ViewBag.PeakBusinessHour = peakBusinessHour;
            ViewBag.HourlyDistribution = hourlyDistribution;

            // 5. Current-month daily revenue chart data
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var dailyRevenueList = new List<decimal>();
            var dailyLabelsList = new List<string>();

            var monthPayments = db.Payments.Where(p => ((p.Status == "Completed" || p.Status == "Refunded") && p.PaymentDate >= monthStart && p.PaymentDate < nextMonthStart) || (p.Status == "Refunded" && p.RefundedAt >= monthStart && p.RefundedAt < nextMonthStart)).ToList();
            for (int day = 1; day <= daysInMonth; day++)
            {
                dailyLabelsList.Add(string.Format("{0:D2}/{1:D2}", day, now.Month));
                var dayOrders = monthValidOrders.Where(o => o.CreatedAt.Day == day);
                dailyRevenueList.Add(RestaurantWebsite.Services.ReportMath.Calculate(monthPayments, monthStart.AddDays(day - 1), monthStart.AddDays(day)).Net);
            }
            ViewBag.DailyLabels = dailyLabelsList;
            ViewBag.DailyRevenues = dailyRevenueList;

            // 6. Order volume by actual order type (this month)
            var orderTypeDistribution = monthValidOrders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.OrderType) ? "Unknown" : o.OrderType)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.OrderTypeDistribution = orderTypeDistribution;

            // 7. Top 5 dishes by quantity sold (all time valid orders)
            var topDishes = db.OrderItems
                .Where(oi => oi.Order.Payments.Any(p => p.Status == "Completed") && oi.DishId.HasValue)
                .GroupBy(oi => oi.Dish.Name)
                .Select(g => new TopDishItem
                {
                    DishName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.SubTotal)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();
            ViewBag.TopDishes = topDishes;

            return View();
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

    public class PeakHourItem
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopDishItem
    {
        public string DishName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}

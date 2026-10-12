using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
{
    public class TopDishItem
    {
        public string DishName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class PeakHourItem
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /admin
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.CurrentRole = Session["Role"];
            ViewBag.UserName = Session["UserName"];

            // Quick totals for summary cards
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalDishes = db.Dishes.Count();
            ViewBag.TotalTables = db.RestaurantTables.Count();
            ViewBag.TotalReservations = db.Reservations.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalPromotions = db.Promotions.Count();
            ViewBag.TotalUsers = db.Users.Count();

            // Analytics dates setup
            DateTime now = DateTime.Now;
            DateTime todayStart = now.Date;
            DateTime tomorrowStart = todayStart.AddDays(1);
            DateTime monthStart = new DateTime(now.Year, now.Month, 1);
            DateTime nextMonthStart = monthStart.AddMonths(1);

            // Valid orders for revenue calculation (exclude Cancelled)
            var validOrdersQuery = db.Orders.Where(o => o.Status != "Cancelled");

            // 1. Today Revenue
            decimal todayRevenue = validOrdersQuery
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < tomorrowStart)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            ViewBag.TodayRevenue = todayRevenue;

            // 2. Current Month Revenue
            decimal monthRevenue = validOrdersQuery
                .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < nextMonthStart)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            ViewBag.MonthRevenue = monthRevenue;

            // 3. Orders Today (All statuses)
            int ordersToday = db.Orders.Count(o => o.CreatedAt >= todayStart && o.CreatedAt < tomorrowStart);
            ViewBag.OrdersToday = ordersToday;

            // Monthly valid orders retrieved in memory once for aggregations (avoids N+1 / complex SQL translation)
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
                ? $"{peakHourObj.Hour:D2}:00 - {peakHourObj.Hour + 1:D2}:00 ({peakHourObj.OrderCount} orders)"
                : "N/A";
            ViewBag.PeakBusinessHour = peakBusinessHour;
            ViewBag.HourlyDistribution = hourlyDistribution;

            // 5. Current-month daily revenue chart data
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var dailyRevenueList = new List<decimal>();
            var dailyLabelsList = new List<string>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                dailyLabelsList.Add($"{day:D2}/{now.Month:D2}");
                var dayOrders = monthValidOrders.Where(o => o.CreatedAt.Day == day);
                dailyRevenueList.Add(dayOrders.Sum(o => o.TotalAmount));
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
                .Where(oi => oi.Order.Status != "Cancelled" && oi.DishId.HasValue)
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
}

using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using RestaurantWebsite.Models;
using RestaurantWebsite.Infrastructure;
namespace RestaurantWebsite.Services
{
    public sealed class ReviewService
    {
        private readonly AppDbContext db;
        public ReviewService(AppDbContext context) { db = context; }
        public void Submit(int userId, int orderId, int dishId, int rating, string comment)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (rating < 1 || rating > 5 || (comment != null && comment.Length > 1000)) throw new InvalidOperationException("Đánh giá cần từ 1 đến 5 sao, bình luận tối đa 1.000 ký tự.");
                var order = db.Orders.Include(o => o.OrderItems).SingleOrDefault(o => o.Id == orderId && o.UserId == userId && o.Status == "Delivered");
                if (order == null || !order.OrderItems.Any(i => i.DishId == dishId)) throw new InvalidOperationException("Chỉ đánh giá món đã mua trong đơn hàng hoàn tất của bạn.");
                if (db.Reviews.Any(r => r.UserId == userId && r.OrderId == orderId && r.DishId == dishId)) throw new InvalidOperationException("Bạn đã đánh giá món này trong đơn hàng.");
                db.Reviews.Add(new Review { UserId = userId, OrderId = orderId, DishId = dishId, Rating = rating, Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(), IsApproved = false, CreatedAt = RestaurantClock.Now });
                db.SaveChanges(); tx.Commit();
            }
        }
    }
}

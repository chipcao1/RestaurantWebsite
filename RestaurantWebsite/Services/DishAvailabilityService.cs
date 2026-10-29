using System;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Services
{
    public sealed class DishAvailabilityService
    {
        private readonly AppDbContext db;

        public DishAvailabilityService(AppDbContext context) { db = context; }

        public void Toggle(int id)
        {
            var dish = db.Dishes.Find(id);
            if (dish == null) throw new InvalidOperationException("Không tìm thấy món ăn.");
            dish.Status = DishAvailabilityRules.NextStatus(dish.Status);
            db.SaveChanges();
        }
    }
}

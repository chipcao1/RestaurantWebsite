using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? DishId { get; set; }

        public int OrderId { get; set; }

        [Range(1, 5, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Số sao")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Bình luận")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsApproved { get; set; }

        public virtual User User { get; set; }

        public virtual Dish Dish { get; set; }

        public virtual Order Order { get; set; }

        public Review()
        {
            CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
            IsApproved = false;
        }
    }
}

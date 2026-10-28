using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Dish
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(150, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Tên")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Range(1, 300, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Thời gian chế biến")]
        public int PreparationTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<DishImage> DishImages { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ComboItem> ComboItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        public Dish()
        {
            DishImages = new HashSet<DishImage>();
            OrderItems = new HashSet<OrderItem>();
            ComboItems = new HashSet<ComboItem>();
            Reviews = new HashSet<Review>();
            Status = "Available";
            CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? ReservationId { get; set; }

        public int? TableId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Hình thức phục vụ")]
        public string OrderType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        public string DeliveryAddress { get; set; }

        [StringLength(20, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Điện thoại")]
        public string Phone { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        public decimal SubTotal { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        public decimal DiscountAmount { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        public decimal DeliveryFee { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        public decimal TotalAmount { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        public string Note { get; set; }

        public DateTime? ServiceStart { get; set; }
        public DateTime? ServiceEnd { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }

        public virtual Reservation Reservation { get; set; }

        public virtual RestaurantTable Table { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }

        public virtual ICollection<OrderPromotion> OrderPromotions { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
            Payments = new HashSet<Payment>();
            OrderPromotions = new HashSet<OrderPromotion>();
            Reviews = new HashSet<Review>();

            Status = "Pending";
            CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
        }
    }
}

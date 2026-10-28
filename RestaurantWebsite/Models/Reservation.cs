using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int UserId { get; set; }


        public int TableId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [Display(Name = "Ngày giờ đặt bàn")]
        public DateTime ReservationDate { get; set; }

        [Range(1, 50, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Số khách")]
        public int PartySize { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Yêu cầu đặc biệt")]
        public string SpecialRequest { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }

        public virtual RestaurantTable Table { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public Reservation()
        {
            Orders = new HashSet<Order>();
            Status = "Pending";
            CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now;
        }
    }
}

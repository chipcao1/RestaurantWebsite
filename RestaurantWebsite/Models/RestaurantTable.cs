using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(20, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Số bàn")]
        public string TableNumber { get; set; }

        [Range(1, 50, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Sức chứa")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(50, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Khu vực")]
        public string Zone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        public RestaurantTable()
        {
            Reservations = new HashSet<Reservation>();
            Orders = new HashSet<Order>();
            Status = "Available";
            IsActive = true;
        }
    }
}

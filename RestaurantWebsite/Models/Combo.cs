using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Combo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(150, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Tên")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Đường dẫn ảnh")]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public virtual ICollection<ComboItem> ComboItems { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public Combo()
        {
            ComboItems = new HashSet<ComboItem>();
            OrderItems = new HashSet<OrderItem>();
            IsActive = true;
        }
    }
}

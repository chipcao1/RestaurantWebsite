using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(50, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Mã khuyến mãi")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(150, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Tên")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Mức giảm")]
        public decimal DiscountValue { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Đơn tối thiểu")]
        public decimal MinimumOrderAmount { get; set; }

        public decimal? MaximumDiscount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<OrderPromotion> OrderPromotions { get; set; }

        public Promotion()
        {
            OrderPromotions = new HashSet<OrderPromotion>();
            IsActive = true;
        }
    }
}

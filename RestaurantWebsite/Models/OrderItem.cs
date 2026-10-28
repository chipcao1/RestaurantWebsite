using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int? DishId { get; set; }

        public int? ComboId { get; set; }

        [Range(1, 100, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        public decimal UnitPrice { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        public string Note { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        public decimal SubTotal { get; set; }

        public virtual Order Order { get; set; }

        public virtual Dish Dish { get; set; }

        public virtual Combo Combo { get; set; }
    }
}

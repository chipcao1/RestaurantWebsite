using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class ComboItem
    {
        public int Id { get; set; }

        public int ComboId { get; set; }

        public int DishId { get; set; }

        [Range(1, 100, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        public virtual Combo Combo { get; set; }

        public virtual Dish Dish { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class DishImage
    {
        public int Id { get; set; }

        public int DishId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Đường dẫn ảnh")]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; }

        public virtual Dish Dish { get; set; }
    }
}

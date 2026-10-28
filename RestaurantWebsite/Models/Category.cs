using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(100, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Tên")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [StringLength(250, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Đường dẫn ảnh")]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Dish> Dishes { get; set; }

        public Category()
        {
            Dishes = new HashSet<Dish>();
            IsActive = true;
        }
    }
}

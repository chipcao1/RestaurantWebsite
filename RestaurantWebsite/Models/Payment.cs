using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWebsite.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Range(0, 999999999, ErrorMessage = "{0} phải từ {1} đến {2}.")]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập {0}.")]
        [StringLength(30, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime? RefundedAt { get; set; }
        public int? ConfirmedById { get; set; }
        public int? RefundedById { get; set; }

        [StringLength(100, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        public string TransactionCode { get; set; }

        public virtual Order Order { get; set; }
    }
}

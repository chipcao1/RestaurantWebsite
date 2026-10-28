using System;
using RestaurantWebsite.Infrastructure;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Services
{
    public static class PromotionRules
    {
        public static decimal Calculate(Promotion promotion, decimal subtotal, DateTime now)
        {
            if (promotion == null || !promotion.IsActive || promotion.StartDate > now || promotion.EndDate < now ||
                (promotion.UsageLimit.HasValue && promotion.UsedCount >= promotion.UsageLimit.Value))
                throw new InvalidOperationException("Mã khuyến mãi không hợp lệ, hết hạn hoặc đã hết lượt sử dụng.");
            if (subtotal < promotion.MinimumOrderAmount)
                throw new InvalidOperationException("Mã " + promotion.Code + " áp dụng cho đơn từ " + Vietnamese.Money(promotion.MinimumOrderAmount) + ".");
            return PricingRules.Discount(subtotal, promotion.DiscountType, promotion.DiscountValue, promotion.MaximumDiscount);
        }

        public static string Describe(Promotion promotion)
        {
            var type = (promotion.DiscountType ?? "").Trim().ToLowerInvariant();
            var value = type == "percentage" || type == "percent"
                ? promotion.DiscountValue.ToString("0.##", Vietnamese.Culture) + "%"
                : Vietnamese.Money(promotion.DiscountValue);
            return "Giảm " + value + (promotion.MaximumDiscount.HasValue ? " (tối đa " + Vietnamese.Money(promotion.MaximumDiscount.Value) + ")" : "");
        }
    }
}

using System;
namespace RestaurantWebsite.Services
{
    public static class PricingRules
    {
        public static decimal Discount(decimal subtotal, string type, decimal value, decimal? maximum)
        {
            if (subtotal < 0 || value < 0 || maximum < 0) throw new InvalidOperationException("Thông tin giá hoặc khuyến mãi không hợp lệ.");
            var normalized = (type ?? "").Trim().ToLowerInvariant(); decimal amount;
            if (normalized == "percentage" || normalized == "percent")
            { if (value > 100) throw new InvalidOperationException("Mức giảm phần trăm không được vượt quá 100%."); amount = subtotal * value / 100; }
            else if (normalized == "fixed" || normalized == "fixed amount" || normalized == "amount") amount = value;
            else throw new InvalidOperationException("Loại khuyến mãi không được hỗ trợ.");
            if (maximum.HasValue) amount = Math.Min(amount, maximum.Value);
            return Math.Min(Math.Min(subtotal, maximum ?? decimal.MaxValue), Math.Round(Math.Min(subtotal, amount), 0, MidpointRounding.AwayFromZero));
        }
    }
}

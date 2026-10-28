using System;
using System.Linq;
using RestaurantWebsite.Models;
namespace RestaurantWebsite.Services
{
    public static class CatalogRules
    {
        public static bool Purchasable(Combo combo, DateTime now)
        {
            return combo != null && combo.IsActive && combo.Price >= 0 &&
                (!combo.StartDate.HasValue || combo.StartDate <= now) && (!combo.EndDate.HasValue || combo.EndDate >= now) &&
                combo.ComboItems.Any() && combo.ComboItems.All(i => i.Quantity > 0 && i.Dish != null && i.Dish.Status == "Available" && i.Dish.Category != null && i.Dish.Category.IsActive);
        }
        public static bool ValidImageUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            if (value.Length > 500 || value.Contains("..") || value.Contains("\\") || value.IndexOfAny(new[] { '\r', '\n' }) >= 0) return false;
            if (value.StartsWith("~/Content/images/", StringComparison.Ordinal) || value.StartsWith("/Content/images/", StringComparison.Ordinal) || value.StartsWith("~/Resources/", StringComparison.Ordinal) || (value.StartsWith("/Resources/", StringComparison.Ordinal))) return true;
            Uri uri; return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo);
        }
    }
}

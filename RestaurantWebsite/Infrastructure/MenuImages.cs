using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RestaurantWebsite.Services;
namespace RestaurantWebsite.Infrastructure
{
    public static class MenuImages
    {
        public const string Placeholder = "~/Content/images/food-placeholder.svg";
        public const string RemoteRoot = "https://raw.githubusercontent.com/chipcao1/RestaurantWebsite/2447b85b446d447c5bd7c051271d5dcccce7b7f9/RestaurantWebsite/Resources/User/images/";
        private static readonly Regex LegacyImage = new Regex(@"^~?/Resources/User/images/(?<file>dish(?:0[1-9]|[12][0-9]|30)\.jpg|combo(?:01|03)\.jpg|f[1-9]\.png|o[12]\.jpg)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly IDictionary<string,string> Dishes = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Gỏi cuốn tôm", "dish01.jpg" },
            { "Spring Rolls", "dish01.jpg" },
            { "Cánh gà chiên", "dish02.jpg" },
            { "Fried Chicken Wings", "dish02.jpg" },
            { "Khoai tây chiên", "dish03.jpg" },
            { "French Fries", "dish03.jpg" },
            { "Há cảo chiên", "dish04.jpg" },
            { "Fried Dumplings", "dish04.jpg" },
            { "Bánh mì bơ tỏi", "dish05.jpg" },
            { "Garlic Bread", "dish05.jpg" },
            { "Bò nướng sốt nấm", "dish06.jpg" },
            { "Grilled Beef Steak", "dish06.jpg" },
            { "Gà nướng thảo mộc", "dish07.jpg" },
            { "Grilled Chicken", "dish07.jpg" },
            { "Cơm sườn nướng", "dish08.jpg" },
            { "Pork Chop", "dish08.jpg" },
            { "Bánh mì kẹp bò", "dish09.jpg" },
            { "Beef Burger", "dish09.jpg" },
            { "Bánh mì kẹp gà", "dish10.jpg" },
            { "Chicken Burger", "dish10.jpg" },
            { "Cơm chiên trứng", "dish11.jpg" },
            { "Fried Rice", "dish11.jpg" },
            { "Cơm chiên bò", "dish12.jpg" },
            { "Beef Fried Rice", "dish12.jpg" },
            { "Cơm chiên gà", "dish13.jpg" },
            { "Chicken Fried Rice", "dish13.jpg" },
            { "Phở bò", "dish14.jpg" },
            { "Beef Noodles", "dish14.jpg" },
            { "Phở gà", "dish15.jpg" },
            { "Chicken Noodles", "dish15.jpg" },
            { "Tôm nướng tỏi", "dish16.jpg" },
            { "Grilled Shrimp", "dish16.jpg" },
            { "Mực chiên giòn", "dish17.jpg" },
            { "Fried Calamari", "dish17.jpg" },
            { "Cá hồi nướng", "dish18.jpg" },
            { "Grilled Salmon", "dish18.jpg" },
            { "Lẩu hải sản", "dish19.jpg" },
            { "Seafood Hotpot", "dish19.jpg" },
            { "Tôm sốt bơ tỏi", "dish20.jpg" },
            { "Butter Garlic Prawns", "dish20.jpg" },
            { "Nước ngọt Coca-Cola", "dish21.jpg" },
            { "Coca Cola", "dish21.jpg" },
            { "Nước cam ép", "dish22.jpg" },
            { "Orange Juice", "dish22.jpg" },
            { "Trà chanh", "dish23.jpg" },
            { "Lemon Tea", "dish23.jpg" },
            { "Cà phê sữa đá", "dish24.jpg" },
            { "Milk Coffee", "dish24.jpg" },
            { "Nước khoáng", "dish25.jpg" },
            { "Mineral Water", "dish25.jpg" },
            { "Bánh phô mai", "dish26.jpg" },
            { "Cheesecake", "dish26.jpg" },
            { "Bánh sô-cô-la", "dish27.jpg" },
            { "Chocolate Cake", "dish27.jpg" },
            { "Kem vani", "dish28.jpg" },
            { "Ice Cream", "dish28.jpg" },
            { "Trái cây theo mùa", "dish29.jpg" },
            { "Fruit Salad", "dish29.jpg" },
            { "Bánh tiramisu", "dish30.jpg" },
            { "Tiramisu", "dish30.jpg" },
        };
        public static string Resolve(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Placeholder;
            value = value.Trim();
            var legacy = LegacyImage.Match(value);
            if (legacy.Success) return RemoteRoot + legacy.Groups["file"].Value.ToLowerInvariant();
            return CatalogRules.ValidImageUrl(value) ? value : Placeholder;
        }
        public static string Dish(string name)
        {
            string file;
            return name != null && Dishes.TryGetValue(name, out file) ? RemoteRoot + file : Placeholder;
        }
        public static string Combo(string name)
        {
            return RemoteRoot + ((name ?? "").IndexOf("gà", StringComparison.OrdinalIgnoreCase) >= 0 || (name ?? "").IndexOf("chicken", StringComparison.OrdinalIgnoreCase) >= 0 ? "combo03.jpg" : "combo01.jpg");
        }
    }
}

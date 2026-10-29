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
            { "Gỏi cuốn tôm", "https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png" },
            { "Spring Rolls", "https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png" },
            { "Cánh gà chiên", "https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg" },
            { "Fried Chicken Wings", "https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg" },
            { "Khoai tây chiên", "https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg" },
            { "French Fries", "https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg" },
            { "Há cảo chiên", "https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp" },
            { "Fried Dumplings", "https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp" },
            { "Bánh mì bơ tỏi", "https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg" },
            { "Garlic Bread", "https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg" },
            { "Bò nướng sốt nấm", "https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg" },
            { "Grilled Beef Steak", "https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg" },
            { "Gà nướng thảo mộc", "https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg" },
            { "Grilled Chicken", "https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg" },
            { "Cơm sườn nướng", "https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg" },
            { "Pork Chop", "https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg" },
            { "Bánh mì kẹp bò", "https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg" },
            { "Beef Burger", "https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg" },
            { "Bánh mì kẹp gà", "https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg" },
            { "Chicken Burger", "https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg" },
            { "Cơm chiên trứng", "https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg" },
            { "Fried Rice", "https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg" },
            { "Cơm chiên bò", "https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png" },
            { "Beef Fried Rice", "https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png" },
            { "Cơm chiên gà", "https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp" },
            { "Chicken Fried Rice", "https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp" },
            { "Phở bò", "https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg" },
            { "Beef Noodles", "https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg" },
            { "Phở gà", "https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg" },
            { "Chicken Noodles", "https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg" },
            { "Tôm nướng tỏi", "https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg" },
            { "Grilled Shrimp", "https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg" },
            { "Mực chiên giòn", "https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg" },
            { "Fried Calamari", "https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg" },
            { "Cá hồi nướng", "https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg" },
            { "Grilled Salmon", "https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg" },
            { "Lẩu hải sản", "https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg" },
            { "Seafood Hotpot", "https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg" },
            { "Tôm sốt bơ tỏi", "https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg" },
            { "Butter Garlic Prawns", "https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg" },
            { "Nước ngọt Coca-Cola", "https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp" },
            { "Coca Cola", "https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp" },
            { "Nước cam ép", "https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg" },
            { "Orange Juice", "https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg" },
            { "Trà chanh", "https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg" },
            { "Lemon Tea", "https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg" },
            { "Cà phê sữa đá", "https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png" },
            { "Milk Coffee", "https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png" },
            { "Nước khoáng", "https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg" },
            { "Mineral Water", "https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg" },
            { "Bánh phô mai", "https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png" },
            { "Cheesecake", "https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png" },
            { "Bánh sô-cô-la", "https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg" },
            { "Chocolate Cake", "https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg" },
            { "Kem vani", "https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg" },
            { "Ice Cream", "https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg" },
            { "Trái cây theo mùa", "https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png" },
            { "Fruit Salad", "https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png" },
            { "Bánh tiramisu", "https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg" },
            { "Tiramisu", "https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg" },
        };
        private static readonly string[] LegacyNames = { "Gỏi cuốn tôm", "Cánh gà chiên", "Khoai tây chiên", "Há cảo chiên", "Bánh mì bơ tỏi", "Bò nướng sốt nấm", "Gà nướng thảo mộc", "Cơm sườn nướng", "Bánh mì kẹp bò", "Bánh mì kẹp gà", "Cơm chiên trứng", "Cơm chiên bò", "Cơm chiên gà", "Phở bò", "Phở gà", "Tôm nướng tỏi", "Mực chiên giòn", "Cá hồi nướng", "Lẩu hải sản", "Tôm sốt bơ tỏi", "Nước ngọt Coca-Cola", "Nước cam ép", "Trà chanh", "Cà phê sữa đá", "Nước khoáng", "Bánh phô mai", "Bánh sô-cô-la", "Kem vani", "Trái cây theo mùa", "Bánh tiramisu" };
        public static bool IsDefault(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.Trim().StartsWith(RemoteRoot, StringComparison.OrdinalIgnoreCase) || LegacyImage.IsMatch(value.Trim());
        }
        public static string ResolveDish(string name, string value)
        {
            return IsDefault(value) ? Dish(name) : Resolve(value);
        }
        public static string ResolveCombo(string name, string value)
        {
            return IsDefault(value) ? Combo(name) : Resolve(value);
        }
        public static string Resolve(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Placeholder;
            value = value.Trim();
            if (value.StartsWith(RemoteRoot, StringComparison.OrdinalIgnoreCase)) value = "/Resources/User/images/" + value.Substring(RemoteRoot.Length);
            var legacy = LegacyImage.Match(value);
            if (legacy.Success)
            {
                string file = legacy.Groups["file"].Value;
                int number;
                if (file.StartsWith("dish", StringComparison.OrdinalIgnoreCase) && int.TryParse(file.Substring(4,2),out number)) return Dish(LegacyNames[number-1]);
                if (file.Equals("combo03.jpg",StringComparison.OrdinalIgnoreCase)) return Combo("Suất gà và nước uống");
                if (file.Equals("combo01.jpg",StringComparison.OrdinalIgnoreCase)) return Combo("Suất ăn gia đình");
                return Placeholder;
            }
            return CatalogRules.ValidImageUrl(value) ? value : Placeholder;
        }
        public static string Dish(string name)
        {
            string url;
            return name != null && Dishes.TryGetValue(name, out url) ? url : Placeholder;
        }
        public static string Combo(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "suất ăn gia đình": case "family combo": case "suất gia đình (mẫu yamy)":
                case "suất gà và nước uống": case "chicken meal combo": case "suất gà (mẫu yamy)": return Dish("Gà nướng thảo mộc");
                case "suất tráng miệng": case "dessert combo": case "suất tráng miệng (mẫu yamy)": return Dish("Bánh phô mai");
                default: return Placeholder;
            }
        }
    }
}

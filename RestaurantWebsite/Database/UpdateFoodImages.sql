/* CẬP NHẬT URL ẢNH MÓN / SUẤT ĂN — giữ nguyên schema main.
   Chọn đúng database trong SSMS rồi Execute. Không chạy Add-Migration.
   Chỉ sửa URL ảnh; không sửa tên, giá, tài khoản, đơn hoặc dữ liệu thanh toán.
   URL mới được chọn theo đúng tên món; nguồn ghi trong food-image-catalog.json.
   Bạn có thể sửa cột ImageUrl trong @DishImages / @ComboImages thành URL HTTPS
   ảnh thật bạn muốn. Để thay cả ảnh HTTPS đang có, đổi cờ dưới đây thành 1.
   Mặc định 0: giữ ảnh HTTPS tùy chỉnh, thay ảnh trống, ảnh mẫu cục bộ và URL ảnh main cũ.
   Cần kết nối internet từ trình duyệt để tải ảnh; khi lỗi web dùng SVG nhẹ.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb')
    THROW 51010,N'Chọn đúng database của RestaurantWebsite trước khi chạy.',1;
IF OBJECT_ID(N'dbo.Dishes',N'U') IS NULL OR OBJECT_ID(N'dbo.DishImages',N'U') IS NULL
    OR OBJECT_ID(N'dbo.Comboes',N'U') IS NULL OR OBJECT_ID(N'dbo.Categories',N'U') IS NULL
    THROW 51011,N'Thiếu bảng schema main. Bảng suất ăn phải là dbo.Comboes.',1;
DECLARE @ReplaceExistingExternal bit = 0;
DECLARE @Root nvarchar(250) = N'https://raw.githubusercontent.com/chipcao1/RestaurantWebsite/2447b85b446d447c5bd7c051271d5dcccce7b7f9/RestaurantWebsite/Resources/User/images/';
DECLARE @Legacy TABLE (LocalUrl nvarchar(250) PRIMARY KEY,RemoteUrl nvarchar(500));
INSERT INTO @Legacy VALUES
(N'/Resources/User/images/combo01.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'/Resources/User/images/combo03.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'/Resources/User/images/dish01.jpg',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'/Resources/User/images/dish02.jpg',N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
(N'/Resources/User/images/dish03.jpg',N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
(N'/Resources/User/images/dish04.jpg',N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
(N'/Resources/User/images/dish05.jpg',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
(N'/Resources/User/images/dish06.jpg',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'/Resources/User/images/dish07.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'/Resources/User/images/dish08.jpg',N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
(N'/Resources/User/images/dish09.jpg',N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
(N'/Resources/User/images/dish10.jpg',N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
(N'/Resources/User/images/dish11.jpg',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'/Resources/User/images/dish12.jpg',N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
(N'/Resources/User/images/dish13.jpg',N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
(N'/Resources/User/images/dish14.jpg',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
(N'/Resources/User/images/dish15.jpg',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
(N'/Resources/User/images/dish16.jpg',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'/Resources/User/images/dish17.jpg',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
(N'/Resources/User/images/dish18.jpg',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
(N'/Resources/User/images/dish19.jpg',N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
(N'/Resources/User/images/dish20.jpg',N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
(N'/Resources/User/images/dish21.jpg',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'/Resources/User/images/dish22.jpg',N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
(N'/Resources/User/images/dish23.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
(N'/Resources/User/images/dish24.jpg',N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
(N'/Resources/User/images/dish25.jpg',N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
(N'/Resources/User/images/dish26.jpg',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'/Resources/User/images/dish27.jpg',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
(N'/Resources/User/images/dish28.jpg',N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
(N'/Resources/User/images/dish29.jpg',N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
(N'/Resources/User/images/dish30.jpg',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg'),
(N'/Resources/User/images/f1.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f2.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f3.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f4.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f5.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f6.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f7.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f8.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f9.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/o1.jpg',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/o2.jpg',N'/Content/images/food-placeholder.svg'),
(N'/Content/images/categories/appetizers.jpg',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'/Content/images/categories/main-dishes.jpg',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'/Content/images/categories/rice-noodles.jpg',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'/Content/images/categories/seafood.jpg',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'/Content/images/categories/drinks.jpg',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'/Content/images/categories/desserts.jpg',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
DECLARE @DishImages TABLE (Name nvarchar(150) PRIMARY KEY,ImageUrl nvarchar(500));
INSERT INTO @DishImages VALUES
(N'Gỏi cuốn tôm',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'Spring Rolls',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'Cánh gà chiên',N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
(N'Fried Chicken Wings',N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
(N'Khoai tây chiên',N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
(N'French Fries',N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
(N'Há cảo chiên',N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
(N'Fried Dumplings',N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
(N'Bánh mì bơ tỏi',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
(N'Garlic Bread',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
(N'Bò nướng sốt nấm',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'Grilled Beef Steak',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'Gà nướng thảo mộc',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Grilled Chicken',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Cơm sườn nướng',N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
(N'Pork Chop',N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
(N'Bánh mì kẹp bò',N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
(N'Beef Burger',N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
(N'Bánh mì kẹp gà',N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
(N'Chicken Burger',N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
(N'Cơm chiên trứng',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'Fried Rice',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'Cơm chiên bò',N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
(N'Beef Fried Rice',N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
(N'Cơm chiên gà',N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
(N'Chicken Fried Rice',N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
(N'Phở bò',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
(N'Beef Noodles',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
(N'Phở gà',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
(N'Chicken Noodles',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
(N'Tôm nướng tỏi',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'Grilled Shrimp',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'Mực chiên giòn',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
(N'Fried Calamari',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
(N'Cá hồi nướng',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
(N'Grilled Salmon',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
(N'Lẩu hải sản',N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
(N'Seafood Hotpot',N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
(N'Tôm sốt bơ tỏi',N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
(N'Butter Garlic Prawns',N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
(N'Nước ngọt Coca-Cola',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'Coca Cola',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'Nước cam ép',N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
(N'Orange Juice',N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
(N'Trà chanh',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
(N'Lemon Tea',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
(N'Cà phê sữa đá',N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
(N'Milk Coffee',N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
(N'Nước khoáng',N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
(N'Mineral Water',N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
(N'Bánh phô mai',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Cheesecake',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Bánh sô-cô-la',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
(N'Chocolate Cake',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
(N'Kem vani',N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
(N'Ice Cream',N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
(N'Trái cây theo mùa',N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
(N'Fruit Salad',N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
(N'Bánh tiramisu',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg'),
(N'Tiramisu',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg');
DECLARE @ComboImages TABLE (Name nvarchar(150) PRIMARY KEY,ImageUrl nvarchar(500));
INSERT INTO @ComboImages VALUES
(N'Suất ăn gia đình',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Family Combo',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất gia đình (mẫu Yamy)',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất gà và nước uống',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Chicken Meal Combo',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất gà (mẫu Yamy)',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất tráng miệng',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Dessert Combo',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Suất tráng miệng (mẫu Yamy)',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
IF EXISTS (SELECT 1 FROM @DishImages WHERE ImageUrl NOT LIKE N'https://%' OR LEN(ImageUrl)>500)
    OR EXISTS (SELECT 1 FROM @ComboImages WHERE ImageUrl NOT LIKE N'https://%' OR LEN(ImageUrl)>500)
    THROW 51012,N'Ảnh mới cần URL HTTPS trực tiếp, tối đa 500 ký tự.',1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @LockResult int;
    EXEC @LockResult=sys.sp_getapplock @Resource=N'YamyFoodImageUrls',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
    IF @LockResult<0 THROW 51013,N'Không lấy được khóa cập nhật URL ảnh, vui lòng thử lại.',1;

    -- Ảnh đại diện theo tên món (tên Việt và tên gốc main đều được hỗ trợ).
    UPDATE i SET ImageUrl=m.ImageUrl
    FROM dbo.DishImages i JOIN dbo.Dishes d ON d.Id=i.DishId JOIN @DishImages m ON m.Name=d.Name
    WHERE ( @ReplaceExistingExternal=1 OR NULLIF(LTRIM(RTRIM(i.ImageUrl)),N'') IS NULL
        OR i.ImageUrl LIKE @Root+N'%' OR EXISTS (SELECT 1 FROM @Legacy l WHERE l.LocalUrl=REPLACE(i.ImageUrl,N'~/',N'/')) );

    -- Món chưa có ảnh: thêm một ảnh đại diện, không tạo trùng khi chạy lại.
    INSERT INTO dbo.DishImages (DishId,ImageUrl,IsPrimary)
    SELECT d.Id,m.ImageUrl,1 FROM dbo.Dishes d JOIN @DishImages m ON m.Name=d.Name
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DishImages i WHERE i.DishId=d.Id);

    -- Đường dẫn mẫu còn lại được đổi sang HTTPS. Các món nhận diện được đã sửa theo tên ở trên.
    UPDATE i SET ImageUrl=l.RemoteUrl FROM dbo.DishImages i JOIN @Legacy l ON l.LocalUrl=REPLACE(i.ImageUrl,N'~/',N'/') OR @Root+REPLACE(l.LocalUrl,N'/Resources/User/images/',N'')=i.ImageUrl;

    UPDATE c SET ImageUrl=m.ImageUrl FROM dbo.Comboes c JOIN @ComboImages m ON m.Name=c.Name
    WHERE @ReplaceExistingExternal=1 OR NULLIF(LTRIM(RTRIM(c.ImageUrl)),N'') IS NULL
        OR c.ImageUrl LIKE @Root+N'%' OR EXISTS (SELECT 1 FROM @Legacy l WHERE l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/'));
    UPDATE c SET ImageUrl=l.RemoteUrl FROM dbo.Comboes c JOIN @Legacy l ON l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/') OR @Root+REPLACE(l.LocalUrl,N'/Resources/User/images/',N'')=c.ImageUrl;

    -- Một số dữ liệu mẫu dùng chung ảnh món cho danh mục: chỉ đổi URL mẫu này.
    UPDATE c SET ImageUrl=l.RemoteUrl FROM dbo.Categories c JOIN @Legacy l ON l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/') OR @Root+REPLACE(l.LocalUrl,N'/Resources/User/images/',N'')=c.ImageUrl;
    COMMIT TRANSACTION;
    PRINT N'Đã cập nhật URL ảnh. Giá, tài khoản, đơn và schema được giữ nguyên.';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

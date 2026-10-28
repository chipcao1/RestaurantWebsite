using System.Data.Entity.Migrations;
namespace RestaurantWebsite.Migrations
{
    public partial class YamyWorkflow : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.Categories SET Name=N'Khai vị', Description=N'Các món khai vị tươi ngon.' WHERE Name=N'Appetizers' AND Description=N'Fresh appetizers & starters'");
            Sql("UPDATE dbo.Categories SET Name=N'Món chính', Description=N'Món chính từ thịt bò, heo và gà.' WHERE Name=N'Main Dishes' AND Description=N'Hearty meat & poultry entrees'");
            Sql("UPDATE dbo.Categories SET Name=N'Cơm và phở', Description=N'Các món cơm và phở truyền thống.' WHERE Name=N'Rice & Noodles' AND Description=N'Traditional rice & noodle dishes'");
            Sql("UPDATE dbo.Categories SET Name=N'Hải sản', Description=N'Hải sản tươi ngon từ biển.' WHERE Name=N'Seafood' AND Description=N'Fresh coastal seafood delicacies'");
            Sql("UPDATE dbo.Categories SET Name=N'Đồ uống', Description=N'Nước giải khát và nước ép trái cây.' WHERE Name=N'Drinks' AND Description=N'Refreshing beverages & juices'");
            Sql("UPDATE dbo.Categories SET Name=N'Tráng miệng', Description=N'Bánh ngọt và các món tráng miệng mát lạnh.' WHERE Name=N'Desserts' AND Description=N'Sweet cakes & cold treats'");
            Sql("UPDATE dbo.Dishes SET Name=N'Gỏi cuốn tôm', Description=N'Gỏi cuốn tôm tươi kiểu Việt Nam.' WHERE Name=N'Spring Rolls' AND Description=N'Fresh Vietnamese spring rolls with shrimp.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cánh gà chiên', Description=N'Cánh gà giòn dùng cùng sốt tương ớt ngọt.' WHERE Name=N'Fried Chicken Wings' AND Description=N'Crispy wings served with sweet chili sauce.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Khoai tây chiên', Description=N'Khoai tây chiên vàng giòn.' WHERE Name=N'French Fries' AND Description=N'Crispy golden french fries.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Há cảo chiên', Description=N'Há cảo nhân thịt heo đậm vị.' WHERE Name=N'Fried Dumplings' AND Description=N'Savory pork dumplings.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bánh mì bơ tỏi', Description=N'Bánh mì nướng phủ bơ tỏi thơm lừng.' WHERE Name=N'Garlic Bread' AND Description=N'Toasted bread with garlic butter sauce.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bò nướng sốt nấm', Description=N'Thịt bò nướng mềm dùng cùng sốt nấm.' WHERE Name=N'Grilled Beef Steak' AND Description=N'Tender beef steak with mushroom sauce.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Gà nướng thảo mộc', Description=N'Gà nướng thơm hương thảo mộc.' WHERE Name=N'Grilled Chicken' AND Description=N'Grilled chicken with herbs.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cơm sườn nướng', Description=N'Sườn heo nướng mọng nước dùng cùng cơm.' WHERE Name=N'Pork Chop' AND Description=N'Juicy pork chop with rice.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bánh mì kẹp bò', Description=N'Bánh mì kẹp thịt bò và phô mai.' WHERE Name=N'Beef Burger' AND Description=N'Classic beef burger with cheese.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bánh mì kẹp gà', Description=N'Bánh mì kẹp gà chiên giòn.' WHERE Name=N'Chicken Burger' AND Description=N'Crispy chicken burger.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cơm chiên trứng', Description=N'Cơm chiên cùng trứng và rau củ.' WHERE Name=N'Fried Rice' AND Description=N'Fried rice with eggs and vegetables.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cơm chiên bò', Description=N'Cơm chiên đặc biệt cùng thịt bò mềm.' WHERE Name=N'Beef Fried Rice' AND Description=N'Special fried rice with tender beef.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cơm chiên gà', Description=N'Cơm chiên thịt gà đậm đà.' WHERE Name=N'Chicken Fried Rice' AND Description=N'Savory chicken fried rice.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Phở bò', Description=N'Phở bò truyền thống Việt Nam.' WHERE Name=N'Beef Noodles' AND Description=N'Vietnamese beef pho noodles.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Phở gà', Description=N'Phở dùng cùng thịt gà luộc.' WHERE Name=N'Chicken Noodles' AND Description=N'Rice noodles with poached chicken.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Tôm nướng tỏi', Description=N'Tôm tươi nướng cùng tỏi.' WHERE Name=N'Grilled Shrimp' AND Description=N'Fresh king prawns grilled with garlic.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Mực chiên giòn', Description=N'Khoanh mực chiên vàng giòn.' WHERE Name=N'Fried Calamari' AND Description=N'Crispy fried squid rings.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cá hồi nướng', Description=N'Cá hồi nướng dùng cùng sốt chanh.' WHERE Name=N'Grilled Salmon' AND Description=N'Atlantic salmon with lemon sauce.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Lẩu hải sản', Description=N'Lẩu hải sản dùng cùng rau tươi.' WHERE Name=N'Seafood Hotpot' AND Description=N'Seafood hotpot with vegetables.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Tôm sốt bơ tỏi', Description=N'Tôm áp chảo với sốt bơ tỏi.' WHERE Name=N'Butter Garlic Prawns' AND Description=N'Prawns sautéed in garlic butter.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Nước ngọt Coca-Cola', Description=N'Nước ngọt dùng lạnh.' WHERE Name=N'Coca Cola' AND Description=N'Chilled soda.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Nước cam ép', Description=N'Nước cam tươi ép nguyên chất.' WHERE Name=N'Orange Juice' AND Description=N'Freshly squeezed orange juice.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Trà chanh', Description=N'Trà chanh đá thanh mát.' WHERE Name=N'Lemon Tea' AND Description=N'Iced refreshing lemon tea.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cà phê sữa đá', Description=N'Cà phê sữa đá kiểu Việt Nam.' WHERE Name=N'Milk Coffee' AND Description=N'Vietnamese iced coffee with milk.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Nước khoáng', Description=N'Nước khoáng đóng chai.' WHERE Name=N'Mineral Water' AND Description=N'Bottled water.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bánh phô mai', Description=N'Bánh phô mai phủ quả mọng.' WHERE Name=N'Cheesecake' AND Description=N'Berry cheesecake.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bánh sô-cô-la', Description=N'Bánh sô-cô-la mềm mịn.' WHERE Name=N'Chocolate Cake' AND Description=N'Fudge chocolate cake slice.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Kem vani', Description=N'Kem vani thơm ngọt.' WHERE Name=N'Ice Cream' AND Description=N'Vanilla ice cream scoop.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Trái cây theo mùa', Description=N'Trái cây tươi tuyển chọn theo mùa.' WHERE Name=N'Fruit Salad' AND Description=N'Seasonal fresh fruit bowl.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Bánh tiramisu', Description=N'Bánh cà phê tiramisu kiểu Ý.' WHERE Name=N'Tiramisu' AND Description=N'Classic Italian coffee cake.'");
            Sql("UPDATE dbo.Combos SET Name=N'Suất ăn gia đình', Description=N'Suất ăn đầy đủ dành cho cả gia đình.' WHERE Name=N'Family Combo' AND Description=N'Complete combo for family.'");
            Sql("UPDATE dbo.Combos SET Name=N'Suất gà và nước uống', Description=N'Suất gà dùng cùng khoai tây chiên và nước uống.' WHERE Name=N'Chicken Meal Combo' AND Description=N'Chicken meal with fries & drink.'");
            Sql("UPDATE dbo.Combos SET Name=N'Suất tráng miệng', Description=N'Kết hợp các món tráng miệng ngọt ngào.' WHERE Name=N'Dessert Combo' AND Description=N'Sweet dessert treat combo.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Chào khách mới - giảm 10%', Description=N'Giảm 10% giá trị đơn hàng.' WHERE Code=N'WELCOME10' AND Name=N'Welcome 10%' AND Description=N'10% discount for orders.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Tiết kiệm 50.000 đồng', Description=N'Giảm 50.000 đồng cho đơn hàng.' WHERE Code=N'SAVE50K' AND Name=N'Save 50K' AND Description=N'50,000 VND discount.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Cuối tuần - giảm 15%', Description=N'Giảm 15% cho đơn hàng cuối tuần.' WHERE Code=N'WEEKEND15' AND Name=N'Weekend 15%' AND Description=N'15% weekend discount.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Khách thân thiết - giảm 20%', Description=N'Giảm 20% dành cho khách hàng thân thiết.' WHERE Code=N'VIP20' AND Name=N'VIP Member 20%' AND Description=N'20% discount for VIP customers.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Ưu đãi 30.000 đồng', Description=N'Giảm 30.000 đồng trên giá trị đơn hàng.' WHERE Code=N'FREESHIP' AND Name=N'Free Shipping 30K' AND Description=N'30,000 VND delivery discount.'");
            AddColumn("dbo.Orders", "ServiceStart", c => c.DateTime());
            AddColumn("dbo.Orders", "ServiceEnd", c => c.DateTime());
            AddColumn("dbo.Reservations", "AssignedStaffId", c => c.Int());
            AddColumn("dbo.Payments", "RefundedAt", c => c.DateTime());
            AddColumn("dbo.Payments", "ConfirmedById", c => c.Int());
            AddColumn("dbo.Payments", "RefundedById", c => c.Int());
        }
        public override void Down()
        {
            Sql("UPDATE dbo.Promotions SET Name=N'Welcome 10%', Description=N'10% discount for orders.' WHERE Code=N'WELCOME10' AND Name=N'Chào khách mới - giảm 10%' AND Description=N'Giảm 10% giá trị đơn hàng.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Save 50K', Description=N'50,000 VND discount.' WHERE Code=N'SAVE50K' AND Name=N'Tiết kiệm 50.000 đồng' AND Description=N'Giảm 50.000 đồng cho đơn hàng.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Weekend 15%', Description=N'15% weekend discount.' WHERE Code=N'WEEKEND15' AND Name=N'Cuối tuần - giảm 15%' AND Description=N'Giảm 15% cho đơn hàng cuối tuần.'");
            Sql("UPDATE dbo.Promotions SET Name=N'VIP Member 20%', Description=N'20% discount for VIP customers.' WHERE Code=N'VIP20' AND Name=N'Khách thân thiết - giảm 20%' AND Description=N'Giảm 20% dành cho khách hàng thân thiết.'");
            Sql("UPDATE dbo.Promotions SET Name=N'Free Shipping 30K', Description=N'30,000 VND delivery discount.' WHERE Code=N'FREESHIP' AND Name=N'Ưu đãi 30.000 đồng' AND Description=N'Giảm 30.000 đồng trên giá trị đơn hàng.'");
            Sql("UPDATE dbo.Categories SET Name=N'Appetizers', Description=N'Fresh appetizers & starters' WHERE Name=N'Khai vị' AND Description=N'Các món khai vị tươi ngon.'");
            Sql("UPDATE dbo.Categories SET Name=N'Main Dishes', Description=N'Hearty meat & poultry entrees' WHERE Name=N'Món chính' AND Description=N'Món chính từ thịt bò, heo và gà.'");
            Sql("UPDATE dbo.Categories SET Name=N'Rice & Noodles', Description=N'Traditional rice & noodle dishes' WHERE Name=N'Cơm và phở' AND Description=N'Các món cơm và phở truyền thống.'");
            Sql("UPDATE dbo.Categories SET Name=N'Seafood', Description=N'Fresh coastal seafood delicacies' WHERE Name=N'Hải sản' AND Description=N'Hải sản tươi ngon từ biển.'");
            Sql("UPDATE dbo.Categories SET Name=N'Drinks', Description=N'Refreshing beverages & juices' WHERE Name=N'Đồ uống' AND Description=N'Nước giải khát và nước ép trái cây.'");
            Sql("UPDATE dbo.Categories SET Name=N'Desserts', Description=N'Sweet cakes & cold treats' WHERE Name=N'Tráng miệng' AND Description=N'Bánh ngọt và các món tráng miệng mát lạnh.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Spring Rolls', Description=N'Fresh Vietnamese spring rolls with shrimp.' WHERE Name=N'Gỏi cuốn tôm' AND Description=N'Gỏi cuốn tôm tươi kiểu Việt Nam.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Fried Chicken Wings', Description=N'Crispy wings served with sweet chili sauce.' WHERE Name=N'Cánh gà chiên' AND Description=N'Cánh gà giòn dùng cùng sốt tương ớt ngọt.'");
            Sql("UPDATE dbo.Dishes SET Name=N'French Fries', Description=N'Crispy golden french fries.' WHERE Name=N'Khoai tây chiên' AND Description=N'Khoai tây chiên vàng giòn.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Fried Dumplings', Description=N'Savory pork dumplings.' WHERE Name=N'Há cảo chiên' AND Description=N'Há cảo nhân thịt heo đậm vị.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Garlic Bread', Description=N'Toasted bread with garlic butter sauce.' WHERE Name=N'Bánh mì bơ tỏi' AND Description=N'Bánh mì nướng phủ bơ tỏi thơm lừng.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Grilled Beef Steak', Description=N'Tender beef steak with mushroom sauce.' WHERE Name=N'Bò nướng sốt nấm' AND Description=N'Thịt bò nướng mềm dùng cùng sốt nấm.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Grilled Chicken', Description=N'Grilled chicken with herbs.' WHERE Name=N'Gà nướng thảo mộc' AND Description=N'Gà nướng thơm hương thảo mộc.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Pork Chop', Description=N'Juicy pork chop with rice.' WHERE Name=N'Cơm sườn nướng' AND Description=N'Sườn heo nướng mọng nước dùng cùng cơm.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Beef Burger', Description=N'Classic beef burger with cheese.' WHERE Name=N'Bánh mì kẹp bò' AND Description=N'Bánh mì kẹp thịt bò và phô mai.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Chicken Burger', Description=N'Crispy chicken burger.' WHERE Name=N'Bánh mì kẹp gà' AND Description=N'Bánh mì kẹp gà chiên giòn.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Fried Rice', Description=N'Fried rice with eggs and vegetables.' WHERE Name=N'Cơm chiên trứng' AND Description=N'Cơm chiên cùng trứng và rau củ.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Beef Fried Rice', Description=N'Special fried rice with tender beef.' WHERE Name=N'Cơm chiên bò' AND Description=N'Cơm chiên đặc biệt cùng thịt bò mềm.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Chicken Fried Rice', Description=N'Savory chicken fried rice.' WHERE Name=N'Cơm chiên gà' AND Description=N'Cơm chiên thịt gà đậm đà.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Beef Noodles', Description=N'Vietnamese beef pho noodles.' WHERE Name=N'Phở bò' AND Description=N'Phở bò truyền thống Việt Nam.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Chicken Noodles', Description=N'Rice noodles with poached chicken.' WHERE Name=N'Phở gà' AND Description=N'Phở dùng cùng thịt gà luộc.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Grilled Shrimp', Description=N'Fresh king prawns grilled with garlic.' WHERE Name=N'Tôm nướng tỏi' AND Description=N'Tôm tươi nướng cùng tỏi.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Fried Calamari', Description=N'Crispy fried squid rings.' WHERE Name=N'Mực chiên giòn' AND Description=N'Khoanh mực chiên vàng giòn.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Grilled Salmon', Description=N'Atlantic salmon with lemon sauce.' WHERE Name=N'Cá hồi nướng' AND Description=N'Cá hồi nướng dùng cùng sốt chanh.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Seafood Hotpot', Description=N'Seafood hotpot with vegetables.' WHERE Name=N'Lẩu hải sản' AND Description=N'Lẩu hải sản dùng cùng rau tươi.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Butter Garlic Prawns', Description=N'Prawns sautéed in garlic butter.' WHERE Name=N'Tôm sốt bơ tỏi' AND Description=N'Tôm áp chảo với sốt bơ tỏi.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Coca Cola', Description=N'Chilled soda.' WHERE Name=N'Nước ngọt Coca-Cola' AND Description=N'Nước ngọt dùng lạnh.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Orange Juice', Description=N'Freshly squeezed orange juice.' WHERE Name=N'Nước cam ép' AND Description=N'Nước cam tươi ép nguyên chất.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Lemon Tea', Description=N'Iced refreshing lemon tea.' WHERE Name=N'Trà chanh' AND Description=N'Trà chanh đá thanh mát.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Milk Coffee', Description=N'Vietnamese iced coffee with milk.' WHERE Name=N'Cà phê sữa đá' AND Description=N'Cà phê sữa đá kiểu Việt Nam.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Mineral Water', Description=N'Bottled water.' WHERE Name=N'Nước khoáng' AND Description=N'Nước khoáng đóng chai.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Cheesecake', Description=N'Berry cheesecake.' WHERE Name=N'Bánh phô mai' AND Description=N'Bánh phô mai phủ quả mọng.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Chocolate Cake', Description=N'Fudge chocolate cake slice.' WHERE Name=N'Bánh sô-cô-la' AND Description=N'Bánh sô-cô-la mềm mịn.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Ice Cream', Description=N'Vanilla ice cream scoop.' WHERE Name=N'Kem vani' AND Description=N'Kem vani thơm ngọt.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Fruit Salad', Description=N'Seasonal fresh fruit bowl.' WHERE Name=N'Trái cây theo mùa' AND Description=N'Trái cây tươi tuyển chọn theo mùa.'");
            Sql("UPDATE dbo.Dishes SET Name=N'Tiramisu', Description=N'Classic Italian coffee cake.' WHERE Name=N'Bánh tiramisu' AND Description=N'Bánh cà phê tiramisu kiểu Ý.'");
            Sql("UPDATE dbo.Combos SET Name=N'Family Combo', Description=N'Complete combo for family.' WHERE Name=N'Suất ăn gia đình' AND Description=N'Suất ăn đầy đủ dành cho cả gia đình.'");
            Sql("UPDATE dbo.Combos SET Name=N'Chicken Meal Combo', Description=N'Chicken meal with fries & drink.' WHERE Name=N'Suất gà và nước uống' AND Description=N'Suất gà dùng cùng khoai tây chiên và nước uống.'");
            Sql("UPDATE dbo.Combos SET Name=N'Dessert Combo', Description=N'Sweet dessert treat combo.' WHERE Name=N'Suất tráng miệng' AND Description=N'Kết hợp các món tráng miệng ngọt ngào.'");
            DropColumn("dbo.Payments", "RefundedById");
            DropColumn("dbo.Payments", "ConfirmedById");
            DropColumn("dbo.Payments", "RefundedAt");
            DropColumn("dbo.Reservations", "AssignedStaffId");
            DropColumn("dbo.Orders", "ServiceEnd");
            DropColumn("dbo.Orders", "ServiceStart");
        }
    }
}

/*
  DỮ LIỆU MẪU YAMY — SQL Server 2012 trở lên, schema main / InitialCreate.
  Chạy Update-Database trước để tạo bảng, rồi chọn đúng database trong SSMS.
  File này chỉ INSERT dữ liệu, không tạo/đổi/xóa bảng, không sửa lịch sử migration.
  Chạy lại không thêm trùng các bản ghi mẫu còn nguyên khóa nhận diện.
  Không ghi đè mật khẩu, giá, trạng thái hoặc nội dung bản ghi đang có.

  Tài khoản minh họa (chỉ dùng ở môi trường thử nghiệm):
    admin.yamy@example.test   — Quản trị viên
    staff.yamy@example.test   — Nhân viên
    khach1.yamy@example.test  — Khách hàng
    khach2.yamy@example.test  — Khách hàng
  Mật khẩu chung: YamyDemo@2026!
  Hash PBKDF2 tương thích PasswordHasher của ứng dụng, salt riêng mỗi tài khoản.
  Tài khoản đã tồn tại sẽ giữ nguyên mật khẩu cũ.

  Nội dung tiếng Việt; mã role/trạng thái giữ nguyên để ứng dụng xử lý.
  Thời gian tính theo Việt Nam (UTC+7), tương thích dữ liệu main.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51000, N'Hãy chọn database RestaurantWebsite trong SSMS trước khi chạy.', 1;

DECLARE @RequiredTables TABLE (Name sysname NOT NULL);
INSERT INTO @RequiredTables VALUES
(N'Users'), (N'Categories'), (N'Dishes'), (N'DishImages'), (N'RestaurantTables'),
(N'Comboes'), (N'ComboItems'), (N'Promotions'), (N'Reservations'), (N'Orders'),
(N'OrderItems'), (N'Payments'), (N'OrderPromotions'), (N'Reviews');
IF EXISTS (SELECT 1 FROM @RequiredTables WHERE OBJECT_ID(N'dbo.' + Name, N'U') IS NULL)
    THROW 51001, N'Thiếu bảng của schema main. Chạy Update-Database trước; bảng suất ăn phải là dbo.Comboes.', 1;

DECLARE @Now datetime = DATEADD(HOUR, 7, GETUTCDATE());
DECLARE @Today datetime = CAST(CAST(@Now AS date) AS datetime);

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @LockResult int;
    EXEC @LockResult = sys.sp_getapplock @Resource=N'YamyDemoSeed',
        @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=15000;
    IF @LockResult < 0 THROW 51002, N'Không lấy được khóa khởi tạo dữ liệu mẫu. Vui lòng thử lại.', 1;

    -- 1. Tài khoản: không thay mật khẩu của tài khoản đã tồn tại.
    DECLARE @UserSeed TABLE (Email nvarchar(150), FullName nvarchar(100),
        Role nvarchar(30), Phone nvarchar(20), PasswordHash nvarchar(max));
    INSERT INTO @UserSeed VALUES
    (N'admin.yamy@example.test', N'Quản trị viên mẫu', N'Admin', N'0901000001', N'$PBKDF2$V1$50000$WGyOseWNC9SdFAYs+q/mEg==$jWOrqJHOPqruTe8EK44Ro2+VS8rPNQsY4EJT4hRTN0k='),
    (N'staff.yamy@example.test', N'Nhân viên mẫu', N'Staff', N'0901000002', N'$PBKDF2$V1$50000$n1XA3zvLfD+uFSe+/SHsXQ==$X9hLrU7djehNbhyPN41mBfvWSJX2zy9Ig+CSgRSGKB4='),
    (N'khach1.yamy@example.test', N'Nguyễn An', N'Customer', N'0901000003', N'$PBKDF2$V1$50000$Io+8XOKOrgrJAoG2Hixvaw==$YgGMvjhApP/VJ0ceRe4v7PvoZPMVudHt/nlb6hJVAn8='),
    (N'khach2.yamy@example.test', N'Trần Bình', N'Customer', N'0901000004', N'$PBKDF2$V1$50000$lVkKblsQprdEGk+lCy8CiQ==$TDUCbPpx20DwEudxc8QTA3bX+a2HOfJSnheqRN92c1o=');

    IF EXISTS (SELECT 1 FROM dbo.Users u JOIN @UserSeed s ON s.Email=u.Email WHERE u.Role<>s.Role)
        THROW 51003, N'Email mẫu đã có vai trò khác. Không thay đổi tài khoản hiện có.', 1;
    INSERT INTO dbo.Users (Email, FullName, Role, Phone, PasswordHash, Address, IsActive, CreatedAt)
    SELECT s.Email, s.FullName, s.Role, s.Phone, s.PasswordHash,
        N'123 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh', 1, @Now
    FROM @UserSeed s WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Email=s.Email);

    -- 2. Danh mục và 30 món: giữ nguyên các bản ghi cùng tên đã có.
    DECLARE @CategorySeed TABLE (Name nvarchar(100), Description nvarchar(500), ImageUrl nvarchar(250));
    INSERT INTO @CategorySeed VALUES
    (N'Khai vị', N'Các món khai vị tươi ngon.', N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
    (N'Món chính', N'Món chính từ thịt bò, heo và gà.', N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
    (N'Cơm và phở', N'Các món cơm và phở truyền thống.', N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
    (N'Hải sản', N'Hải sản tươi ngon từ biển.', N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
    (N'Đồ uống', N'Nước giải khát và nước ép trái cây.', N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
    (N'Tráng miệng', N'Bánh ngọt và các món tráng miệng mát lạnh.', N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
    INSERT INTO dbo.Categories (Name, Description, ImageUrl, IsActive)
    SELECT s.Name, s.Description, s.ImageUrl, 1 FROM @CategorySeed s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Name=s.Name);

    DECLARE @DishSeed TABLE (Name nvarchar(150), Description nvarchar(1000),
        Price decimal(18,2), PreparationTime int, CategoryName nvarchar(100), ImageUrl nvarchar(500));
    INSERT INTO @DishSeed VALUES
    (N'Gỏi cuốn tôm', N'Gỏi cuốn tôm tươi kiểu Việt Nam.', 45000, 10, N'Khai vị', N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
    (N'Cánh gà chiên', N'Cánh gà giòn dùng cùng sốt tương ớt ngọt.', 65000, 15, N'Khai vị', N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
    (N'Khoai tây chiên', N'Khoai tây chiên vàng giòn.', 35000, 10, N'Khai vị', N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
    (N'Há cảo chiên', N'Há cảo nhân thịt heo đậm vị.', 50000, 12, N'Khai vị', N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
    (N'Bánh mì bơ tỏi', N'Bánh mì nướng phủ bơ tỏi thơm lừng.', 40000, 8, N'Khai vị', N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
    (N'Bò nướng sốt nấm', N'Thịt bò nướng mềm dùng cùng sốt nấm.', 189000, 25, N'Món chính', N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
    (N'Gà nướng thảo mộc', N'Gà nướng thơm hương thảo mộc.', 99000, 20, N'Món chính', N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
    (N'Cơm sườn nướng', N'Sườn heo nướng mọng nước dùng cùng cơm.', 119000, 22, N'Món chính', N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
    (N'Bánh mì kẹp bò', N'Bánh mì kẹp thịt bò và phô mai.', 89000, 15, N'Món chính', N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
    (N'Bánh mì kẹp gà', N'Bánh mì kẹp gà chiên giòn.', 79000, 15, N'Món chính', N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
    (N'Cơm chiên trứng', N'Cơm chiên cùng trứng và rau củ.', 65000, 12, N'Cơm và phở', N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
    (N'Cơm chiên bò', N'Cơm chiên đặc biệt cùng thịt bò mềm.', 85000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
    (N'Cơm chiên gà', N'Cơm chiên thịt gà đậm đà.', 75000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
    (N'Phở bò', N'Phở bò truyền thống Việt Nam.', 79000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
    (N'Phở gà', N'Phở dùng cùng thịt gà luộc.', 69000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
    (N'Tôm nướng tỏi', N'Tôm tươi nướng cùng tỏi.', 149000, 20, N'Hải sản', N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
    (N'Mực chiên giòn', N'Khoanh mực chiên vàng giòn.', 129000, 18, N'Hải sản', N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
    (N'Cá hồi nướng', N'Cá hồi nướng dùng cùng sốt chanh.', 199000, 25, N'Hải sản', N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
    (N'Lẩu hải sản', N'Lẩu hải sản dùng cùng rau tươi.', 249000, 30, N'Hải sản', N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
    (N'Tôm sốt bơ tỏi', N'Tôm áp chảo với sốt bơ tỏi.', 169000, 20, N'Hải sản', N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
    (N'Nước ngọt Coca-Cola', N'Nước ngọt dùng lạnh.', 20000, 2, N'Đồ uống', N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
    (N'Nước cam ép', N'Nước cam tươi ép nguyên chất.', 35000, 5, N'Đồ uống', N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
    (N'Trà chanh', N'Trà chanh đá thanh mát.', 30000, 5, N'Đồ uống', N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
    (N'Cà phê sữa đá', N'Cà phê sữa đá kiểu Việt Nam.', 35000, 5, N'Đồ uống', N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
    (N'Nước khoáng', N'Nước khoáng đóng chai.', 15000, 1, N'Đồ uống', N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
    (N'Bánh phô mai', N'Bánh phô mai phủ quả mọng.', 55000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
    (N'Bánh sô-cô-la', N'Bánh sô-cô-la mềm mịn.', 50000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
    (N'Kem vani', N'Kem vani thơm ngọt.', 40000, 3, N'Tráng miệng', N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
    (N'Trái cây theo mùa', N'Trái cây tươi tuyển chọn theo mùa.', 45000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
    (N'Bánh tiramisu', N'Bánh cà phê tiramisu kiểu Ý.', 65000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg');
    DECLARE @NewDishes TABLE (Id int, Name nvarchar(150));
    INSERT INTO dbo.Dishes (Name, Description, Price, PreparationTime, Status, CategoryId, CreatedAt)
    OUTPUT inserted.Id, inserted.Name INTO @NewDishes
    SELECT s.Name, s.Description, s.Price, s.PreparationTime, N'Available', c.Id, @Now
    FROM @DishSeed s
    CROSS APPLY (SELECT TOP (1) Id FROM dbo.Categories WHERE Name=s.CategoryName ORDER BY Id) c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Dishes d WHERE d.Name=s.Name);
    INSERT INTO dbo.DishImages (DishId, ImageUrl, IsPrimary)
    SELECT d.Id, s.ImageUrl, 1 FROM @NewDishes d JOIN @DishSeed s ON s.Name=d.Name;

    -- 3. Bàn mẫu riêng; không dùng hoặc sửa trạng thái bàn đang có của nhà hàng.
    DECLARE @TableSeed TABLE (TableNumber nvarchar(20), Capacity int, Zone nvarchar(50));
    INSERT INTO @TableSeed VALUES
    (N'YM01',2,N'Indoor'), (N'YM02',2,N'Indoor'), (N'YM03',4,N'Indoor'),
    (N'YM04',4,N'Indoor'), (N'YM05',4,N'Window'), (N'YM06',6,N'Window'),
    (N'YM07',6,N'Outdoor'), (N'YM08',8,N'Outdoor'), (N'YM09',8,N'VIP'), (N'YM10',10,N'VIP');
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity, Zone, Status, IsActive)
    SELECT s.TableNumber, s.Capacity, s.Zone, N'Available', 1 FROM @TableSeed s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables t WHERE t.TableNumber=s.TableNumber);

    -- 4. Suất ăn: tên bảng chính xác là Comboes theo migration main.
    DECLARE @ComboSeed TABLE (Name nvarchar(150), Description nvarchar(500), Price decimal(18,2), ImageUrl nvarchar(500));
    INSERT INTO @ComboSeed VALUES
    (N'Suất gia đình (mẫu Yamy)',N'Gỏi cuốn, gà nướng, cơm chiên và nước ngọt.',299000,N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
    (N'Suất gà (mẫu Yamy)',N'Gà nướng, khoai tây chiên và nước ngọt.',139000,N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
    (N'Suất tráng miệng (mẫu Yamy)',N'Bánh phô mai, bánh sô-cô-la và nước cam.',99000,N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
    DECLARE @NewCombos TABLE (Id int, Name nvarchar(150));
    INSERT INTO dbo.Comboes (Name, Description, Price, ImageUrl, IsActive, StartDate, EndDate)
    OUTPUT inserted.Id, inserted.Name INTO @NewCombos
    SELECT s.Name, s.Description, s.Price, s.ImageUrl, 1, DATEADD(DAY,-1,@Now), DATEADD(MONTH,6,@Now)
    FROM @ComboSeed s WHERE NOT EXISTS (SELECT 1 FROM dbo.Comboes c WHERE c.Name=s.Name);
    DECLARE @ComboLineSeed TABLE (ComboName nvarchar(150), DishName nvarchar(150), Quantity int);
    INSERT INTO @ComboLineSeed VALUES
    (N'Suất gia đình (mẫu Yamy)',N'Gỏi cuốn tôm',1),
    (N'Suất gia đình (mẫu Yamy)',N'Gà nướng thảo mộc',2),
    (N'Suất gia đình (mẫu Yamy)',N'Cơm chiên trứng',2),
    (N'Suất gia đình (mẫu Yamy)',N'Nước ngọt Coca-Cola',2),
    (N'Suất gà (mẫu Yamy)',N'Gà nướng thảo mộc',1),
    (N'Suất gà (mẫu Yamy)',N'Khoai tây chiên',1),
    (N'Suất gà (mẫu Yamy)',N'Nước ngọt Coca-Cola',1),
    (N'Suất tráng miệng (mẫu Yamy)',N'Bánh phô mai',1),
    (N'Suất tráng miệng (mẫu Yamy)',N'Bánh sô-cô-la',1),
    (N'Suất tráng miệng (mẫu Yamy)',N'Nước cam ép',1);
    INSERT INTO dbo.ComboItems (ComboId, DishId, Quantity)
    SELECT c.Id, d.Id, s.Quantity FROM @ComboLineSeed s JOIN @NewCombos c ON c.Name=s.ComboName
    CROSS APPLY (SELECT TOP (1) Id FROM dbo.Dishes WHERE Name=s.DishName ORDER BY Id) d;

    -- 5. Mã khuyến mãi dùng loại Fixed Amount mà ứng dụng hỗ trợ.
    DECLARE @PromoSeed TABLE (Code nvarchar(50), Name nvarchar(150), DiscountType nvarchar(30),
        DiscountValue decimal(18,2), MinimumOrderAmount decimal(18,2), MaximumDiscount decimal(18,2));
    INSERT INTO @PromoSeed VALUES
    (N'YM10',N'Chào khách mới - giảm 10%',N'Percentage',10,100000,50000),
    (N'YM50K',N'Tiết kiệm 50.000 đồng',N'Fixed Amount',50000,300000,50000),
    (N'YM15',N'Ưu đãi cuối tuần - giảm 15%',N'Percentage',15,200000,70000),
    (N'YM20',N'Khách thân thiết - giảm 20%',N'Percentage',20,500000,150000),
    (N'YM30K',N'Ưu đãi 30.000 đồng',N'Fixed Amount',30000,150000,30000);
    DECLARE @NewPromotions TABLE (Id int, Code nvarchar(50));
    DECLARE @UseSamplePromo bit = CASE
        WHEN NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Note=N'[YAMY-DEMO-O01] Ít cay.')
         AND (SELECT TOP (1) Price*2 FROM dbo.Dishes WHERE Name=N'Cơm chiên bò' ORDER BY Id)>=100000
        THEN 1 ELSE 0 END;
    INSERT INTO dbo.Promotions (Code, Name, Description, DiscountType, DiscountValue,
        MinimumOrderAmount, MaximumDiscount, StartDate, EndDate, UsageLimit, UsedCount, IsActive)
    OUTPUT inserted.Id, inserted.Code INTO @NewPromotions
    SELECT s.Code, s.Name, N'Mã khuyến mãi minh họa Yamy.', s.DiscountType, s.DiscountValue,
        s.MinimumOrderAmount, s.MaximumDiscount, DATEADD(DAY,-1,@Now), DATEADD(MONTH,6,@Now), 100,
        CASE WHEN s.Code=N'YM10' AND @UseSamplePromo=1 THEN 1 ELSE 0 END, 1
    FROM @PromoSeed s WHERE NOT EXISTS (SELECT 1 FROM dbo.Promotions p WHERE p.Code=s.Code);

    -- 6. Lịch đặt bàn: mã nhận diện nằm trong yêu cầu đặc biệt.
    DECLARE @Customer1 int = (SELECT TOP (1) Id FROM dbo.Users WHERE Email=N'khach1.yamy@example.test' ORDER BY Id);
    DECLARE @Customer2 int = (SELECT TOP (1) Id FROM dbo.Users WHERE Email=N'khach2.yamy@example.test' ORDER BY Id);
    DECLARE @ReservationSeed TABLE (Marker nvarchar(100), UserId int, TableNumber nvarchar(20),
        ReservationDate datetime, PartySize int, Status nvarchar(30));
    INSERT INTO @ReservationSeed VALUES
    (N'[YAMY-DEMO-R01] Ưu tiên chỗ yên tĩnh.',@Customer1,N'YM01',DATEADD(HOUR,18,DATEADD(DAY,1,@Today)),2,N'Pending'),
    (N'[YAMY-DEMO-R02] Gia đình dùng bữa tối.',@Customer2,N'YM03',DATEADD(HOUR,18,DATEADD(DAY,1,@Today)),4,N'Confirmed'),
    (N'[YAMY-DEMO-R03] Buổi hẹn đã phục vụ.',@Customer1,N'YM05',DATEADD(HOUR,12,DATEADD(DAY,-1,@Today)),2,N'Completed'),
    (N'[YAMY-DEMO-R04] Khách đã hủy lịch.',@Customer2,N'YM07',DATEADD(HOUR,18,DATEADD(DAY,2,@Today)),4,N'Cancelled');
    INSERT INTO dbo.Reservations (UserId, TableId, ReservationDate, PartySize, SpecialRequest, Status, CreatedAt)
    SELECT s.UserId, t.Id, s.ReservationDate, s.PartySize, s.Marker, s.Status, DATEADD(DAY,-2,@Now)
    FROM @ReservationSeed s
    CROSS APPLY (SELECT TOP (1) Id FROM dbo.RestaurantTables WHERE TableNumber=s.TableNumber ORDER BY Id) t
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Reservations r WHERE r.SpecialRequest=s.Marker);

    -- 7. Đơn mẫu ở các trạng thái, giá lấy từ món/suất ăn thực tế.
    -- Tất cả bản ghi con chỉ được thêm lúc tạo đơn, không sửa đơn đã tồn tại.
    DECLARE @OrderSeed TABLE (Marker nvarchar(100), UserId int, OrderType nvarchar(30),
        Status nvarchar(30), CreatedAt datetime, DishName nvarchar(150), Quantity int, ComboName nvarchar(150),
        ReservationMarker nvarchar(100), PaymentStatus nvarchar(30));
    INSERT INTO @OrderSeed VALUES
    (N'[YAMY-DEMO-O01] Ít cay.',@Customer1,N'Takeaway',N'Pending',DATEADD(MINUTE,-30,@Now),N'Cơm chiên bò',2,NULL,NULL,N'Pending'),
    (N'[YAMY-DEMO-O02] Gọi trước khi giao.',@Customer2,N'Delivery',N'Confirmed',DATEADD(MINUTE,-25,@Now),N'Cơm sườn nướng',2,NULL,NULL,N'Pending'),
    (N'[YAMY-DEMO-O03] Không thêm hành.',@Customer1,N'Takeaway',N'Preparing',DATEADD(MINUTE,-20,@Now),N'Phở bò',2,NULL,NULL,N'Pending'),
    (N'[YAMY-DEMO-O04] Khách sắp đến lấy.',@Customer2,N'Takeaway',N'Ready',DATEADD(MINUTE,-15,@Now),N'Gà nướng thảo mộc',1,NULL,NULL,N'Completed'),
    (N'[YAMY-DEMO-O05] Đơn tại bàn đã hoàn tất.',@Customer1,N'Dine-in',N'Delivered',DATEADD(HOUR,12,DATEADD(DAY,-1,@Today)),N'Bò nướng sốt nấm',2,NULL,N'[YAMY-DEMO-R03] Buổi hẹn đã phục vụ.',N'Completed'),
    (N'[YAMY-DEMO-O06] Khách hủy trước khi chế biến.',@Customer2,N'Delivery',N'Cancelled',DATEADD(HOUR,12,DATEADD(DAY,-2,@Today)),N'Phở gà',1,NULL,NULL,N'Failed'),
    (N'[YAMY-DEMO-O07] Đơn suất ăn đã hoàn tất.',@Customer2,N'Takeaway',N'Delivered',DATEADD(HOUR,12,DATEADD(DAY,-3,@Today)),N'Bánh phô mai',1,N'Suất gà (mẫu Yamy)',NULL,N'Completed'),
    (N'[YAMY-DEMO-O08] Đã hoàn tiền và hủy trước chế biến.',@Customer1,N'Takeaway',N'Cancelled',DATEADD(HOUR,12,DATEADD(DAY,-4,@Today)),N'Gỏi cuốn tôm',2,NULL,NULL,N'Refunded');

    DECLARE @Marker nvarchar(100), @UserId int, @OrderType nvarchar(30), @Status nvarchar(30),
        @CreatedAt datetime, @DishName nvarchar(150), @Quantity int, @ComboName nvarchar(150),
        @ReservationMarker nvarchar(100), @PaymentStatus nvarchar(30);
    DECLARE order_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT Marker, UserId, OrderType, Status, CreatedAt, DishName, Quantity, ComboName, ReservationMarker, PaymentStatus
        FROM @OrderSeed ORDER BY Marker;
    OPEN order_cursor;
    FETCH NEXT FROM order_cursor INTO @Marker,@UserId,@OrderType,@Status,@CreatedAt,@DishName,@Quantity,@ComboName,@ReservationMarker,@PaymentStatus;
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Note=@Marker)
        BEGIN
            DECLARE @DishId int, @DishPrice decimal(18,2), @ComboId int, @ComboPrice decimal(18,2),
                @ReservationId int, @TableId int, @SubTotal decimal(18,2), @OrderId int,
                @PromoId int, @Discount decimal(18,2);
            SELECT @DishId=NULL,@DishPrice=NULL,@ComboId=NULL,@ComboPrice=0,@ReservationId=NULL,@TableId=NULL,@PromoId=NULL,@Discount=0;
            SELECT TOP (1) @DishId=Id,@DishPrice=Price FROM dbo.Dishes WHERE Name=@DishName ORDER BY Id;
            IF @ComboName IS NOT NULL
                SELECT TOP (1) @ComboId=Id,@ComboPrice=Price FROM dbo.Comboes WHERE Name=@ComboName ORDER BY Id;
            IF @ReservationMarker IS NOT NULL
                SELECT TOP (1) @ReservationId=Id,@TableId=TableId FROM dbo.Reservations WHERE SpecialRequest=@ReservationMarker ORDER BY Id;
            IF @DishId IS NULL OR (@ComboName IS NOT NULL AND @ComboId IS NULL)
                THROW 51004, N'Không tìm thấy món hoặc suất ăn mẫu.', 1;
            SET @SubTotal=@DishPrice*@Quantity+@ComboPrice;
            -- Chỉ dùng mã YM10 vừa tạo; không đổi UsedCount của mã đã tồn tại.
            IF @Marker=N'[YAMY-DEMO-O01] Ít cay.' AND @SubTotal>=100000
            BEGIN
                SELECT @PromoId=Id FROM @NewPromotions WHERE Code=N'YM10';
                IF @PromoId IS NOT NULL
                    SET @Discount=CASE WHEN ROUND(@SubTotal*0.10,0)>50000 THEN 50000 ELSE ROUND(@SubTotal*0.10,0) END;
            END;
            INSERT INTO dbo.Orders (UserId, ReservationId, TableId, OrderType, Status, DeliveryAddress,
                Phone, SubTotal, DiscountAmount, DeliveryFee, TotalAmount, Note, CreatedAt)
            SELECT @UserId,@ReservationId,@TableId,@OrderType,@Status,
                CASE WHEN @OrderType=N'Delivery' THEN Address ELSE NULL END,
                Phone,@SubTotal,@Discount,0,@SubTotal-@Discount,@Marker,@CreatedAt FROM dbo.Users WHERE Id=@UserId;
            SET @OrderId=CAST(SCOPE_IDENTITY() AS int);
            INSERT INTO dbo.OrderItems (OrderId,DishId,ComboId,Quantity,UnitPrice,Note,SubTotal)
            VALUES (@OrderId,@DishId,NULL,@Quantity,@DishPrice,N'Món minh họa.',@DishPrice*@Quantity);
            IF @ComboId IS NOT NULL
                INSERT INTO dbo.OrderItems (OrderId,DishId,ComboId,Quantity,UnitPrice,Note,SubTotal)
                VALUES (@OrderId,NULL,@ComboId,1,@ComboPrice,N'Suất ăn minh họa.',@ComboPrice);
            IF @PromoId IS NOT NULL
                INSERT INTO dbo.OrderPromotions (OrderId,PromotionId,DiscountAmount)
                VALUES (@OrderId,@PromoId,@Discount);
            INSERT INTO dbo.Payments (OrderId,PaymentMethod,Amount,Status,PaymentDate,TransactionCode)
            VALUES (@OrderId,N'Cash',@SubTotal-@Discount,@PaymentStatus,@CreatedAt,
                N'YAMY-DEMO-'+CAST(@OrderId AS nvarchar(20)));
            -- Không tạo đánh giá cho đơn chưa hoàn tất.
            IF @Status=N'Delivered'
                INSERT INTO dbo.Reviews (UserId,DishId,OrderId,Rating,Comment,CreatedAt,IsApproved)
                VALUES (@UserId,@DishId,@OrderId,5,N'Món ngon, phục vụ chu đáo. Đây là đánh giá minh họa.',DATEADD(HOUR,2,@CreatedAt),
                    CASE WHEN @UserId=@Customer1 THEN 1 ELSE 0 END);
        END;
        FETCH NEXT FROM order_cursor INTO @Marker,@UserId,@OrderType,@Status,@CreatedAt,@DishName,@Quantity,@ComboName,@ReservationMarker,@PaymentStatus;
    END;
    CLOSE order_cursor;
    DEALLOCATE order_cursor;

    COMMIT TRANSACTION;
    PRINT N'Đã thêm dữ liệu mẫu Yamy. Các bản ghi đã tồn tại được giữ nguyên.';
    SELECT Email,FullName,Role,IsActive FROM dbo.Users WHERE Email IN (SELECT Email FROM @UserSeed) ORDER BY Role,Email;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

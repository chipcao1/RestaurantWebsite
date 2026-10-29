/* CẬP NHẬT URL ẢNH MÓN / SUẤT ĂN — giữ nguyên schema main.
   Chọn đúng database trong SSMS rồi Execute. Không chạy Add-Migration.
   Chỉ sửa URL ảnh; không sửa tên, giá, tài khoản, đơn hoặc dữ liệu thanh toán.
   URL mặc định trỏ ảnh gốc của main tại commit cố định trên GitHub.
   Bạn có thể sửa cột ImageUrl trong @DishImages / @ComboImages thành URL HTTPS
   ảnh thật bạn muốn. Để thay cả ảnh HTTPS đang có, đổi cờ dưới đây thành 1.
   Mặc định 0: giữ ảnh HTTPS tùy chỉnh, chỉ thay ảnh trống hoặc ảnh mẫu cục bộ.
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
(N'/Resources/User/images/combo01.jpg',@Root+N'combo01.jpg'),
(N'/Resources/User/images/combo03.jpg',@Root+N'combo03.jpg'),
(N'/Resources/User/images/dish01.jpg',@Root+N'dish01.jpg'),
(N'/Resources/User/images/dish02.jpg',@Root+N'dish02.jpg'),
(N'/Resources/User/images/dish03.jpg',@Root+N'dish03.jpg'),
(N'/Resources/User/images/dish04.jpg',@Root+N'dish04.jpg'),
(N'/Resources/User/images/dish05.jpg',@Root+N'dish05.jpg'),
(N'/Resources/User/images/dish06.jpg',@Root+N'dish06.jpg'),
(N'/Resources/User/images/dish07.jpg',@Root+N'dish07.jpg'),
(N'/Resources/User/images/dish08.jpg',@Root+N'dish08.jpg'),
(N'/Resources/User/images/dish09.jpg',@Root+N'dish09.jpg'),
(N'/Resources/User/images/dish10.jpg',@Root+N'dish10.jpg'),
(N'/Resources/User/images/dish11.jpg',@Root+N'dish11.jpg'),
(N'/Resources/User/images/dish12.jpg',@Root+N'dish12.jpg'),
(N'/Resources/User/images/dish13.jpg',@Root+N'dish13.jpg'),
(N'/Resources/User/images/dish14.jpg',@Root+N'dish14.jpg'),
(N'/Resources/User/images/dish15.jpg',@Root+N'dish15.jpg'),
(N'/Resources/User/images/dish16.jpg',@Root+N'dish16.jpg'),
(N'/Resources/User/images/dish17.jpg',@Root+N'dish17.jpg'),
(N'/Resources/User/images/dish18.jpg',@Root+N'dish18.jpg'),
(N'/Resources/User/images/dish19.jpg',@Root+N'dish19.jpg'),
(N'/Resources/User/images/dish20.jpg',@Root+N'dish20.jpg'),
(N'/Resources/User/images/dish21.jpg',@Root+N'dish21.jpg'),
(N'/Resources/User/images/dish22.jpg',@Root+N'dish22.jpg'),
(N'/Resources/User/images/dish23.jpg',@Root+N'dish23.jpg'),
(N'/Resources/User/images/dish24.jpg',@Root+N'dish24.jpg'),
(N'/Resources/User/images/dish25.jpg',@Root+N'dish25.jpg'),
(N'/Resources/User/images/dish26.jpg',@Root+N'dish26.jpg'),
(N'/Resources/User/images/dish27.jpg',@Root+N'dish27.jpg'),
(N'/Resources/User/images/dish28.jpg',@Root+N'dish28.jpg'),
(N'/Resources/User/images/dish29.jpg',@Root+N'dish29.jpg'),
(N'/Resources/User/images/dish30.jpg',@Root+N'dish30.jpg'),
(N'/Resources/User/images/f1.png',@Root+N'f1.png'),
(N'/Resources/User/images/f2.png',@Root+N'f2.png'),
(N'/Resources/User/images/f3.png',@Root+N'f3.png'),
(N'/Resources/User/images/f4.png',@Root+N'f4.png'),
(N'/Resources/User/images/f5.png',@Root+N'f5.png'),
(N'/Resources/User/images/f6.png',@Root+N'f6.png'),
(N'/Resources/User/images/f7.png',@Root+N'f7.png'),
(N'/Resources/User/images/f8.png',@Root+N'f8.png'),
(N'/Resources/User/images/f9.png',@Root+N'f9.png'),
(N'/Resources/User/images/o1.jpg',@Root+N'o1.jpg'),
(N'/Resources/User/images/o2.jpg',@Root+N'o2.jpg'),
(N'/Content/images/categories/appetizers.jpg',@Root+N'dish01.jpg'),
(N'/Content/images/categories/main-dishes.jpg',@Root+N'dish06.jpg'),
(N'/Content/images/categories/rice-noodles.jpg',@Root+N'dish11.jpg'),
(N'/Content/images/categories/seafood.jpg',@Root+N'dish16.jpg'),
(N'/Content/images/categories/drinks.jpg',@Root+N'dish21.jpg'),
(N'/Content/images/categories/desserts.jpg',@Root+N'dish26.jpg');
DECLARE @DishImages TABLE (Name nvarchar(150) PRIMARY KEY,ImageUrl nvarchar(500));
INSERT INTO @DishImages VALUES
(N'Gỏi cuốn tôm',@Root+N'dish01.jpg'),
(N'Spring Rolls',@Root+N'dish01.jpg'),
(N'Cánh gà chiên',@Root+N'dish02.jpg'),
(N'Fried Chicken Wings',@Root+N'dish02.jpg'),
(N'Khoai tây chiên',@Root+N'dish03.jpg'),
(N'French Fries',@Root+N'dish03.jpg'),
(N'Há cảo chiên',@Root+N'dish04.jpg'),
(N'Fried Dumplings',@Root+N'dish04.jpg'),
(N'Bánh mì bơ tỏi',@Root+N'dish05.jpg'),
(N'Garlic Bread',@Root+N'dish05.jpg'),
(N'Bò nướng sốt nấm',@Root+N'dish06.jpg'),
(N'Grilled Beef Steak',@Root+N'dish06.jpg'),
(N'Gà nướng thảo mộc',@Root+N'dish07.jpg'),
(N'Grilled Chicken',@Root+N'dish07.jpg'),
(N'Cơm sườn nướng',@Root+N'dish08.jpg'),
(N'Pork Chop',@Root+N'dish08.jpg'),
(N'Bánh mì kẹp bò',@Root+N'dish09.jpg'),
(N'Beef Burger',@Root+N'dish09.jpg'),
(N'Bánh mì kẹp gà',@Root+N'dish10.jpg'),
(N'Chicken Burger',@Root+N'dish10.jpg'),
(N'Cơm chiên trứng',@Root+N'dish11.jpg'),
(N'Fried Rice',@Root+N'dish11.jpg'),
(N'Cơm chiên bò',@Root+N'dish12.jpg'),
(N'Beef Fried Rice',@Root+N'dish12.jpg'),
(N'Cơm chiên gà',@Root+N'dish13.jpg'),
(N'Chicken Fried Rice',@Root+N'dish13.jpg'),
(N'Phở bò',@Root+N'dish14.jpg'),
(N'Beef Noodles',@Root+N'dish14.jpg'),
(N'Phở gà',@Root+N'dish15.jpg'),
(N'Chicken Noodles',@Root+N'dish15.jpg'),
(N'Tôm nướng tỏi',@Root+N'dish16.jpg'),
(N'Grilled Shrimp',@Root+N'dish16.jpg'),
(N'Mực chiên giòn',@Root+N'dish17.jpg'),
(N'Fried Calamari',@Root+N'dish17.jpg'),
(N'Cá hồi nướng',@Root+N'dish18.jpg'),
(N'Grilled Salmon',@Root+N'dish18.jpg'),
(N'Lẩu hải sản',@Root+N'dish19.jpg'),
(N'Seafood Hotpot',@Root+N'dish19.jpg'),
(N'Tôm sốt bơ tỏi',@Root+N'dish20.jpg'),
(N'Butter Garlic Prawns',@Root+N'dish20.jpg'),
(N'Nước ngọt Coca-Cola',@Root+N'dish21.jpg'),
(N'Coca Cola',@Root+N'dish21.jpg'),
(N'Nước cam ép',@Root+N'dish22.jpg'),
(N'Orange Juice',@Root+N'dish22.jpg'),
(N'Trà chanh',@Root+N'dish23.jpg'),
(N'Lemon Tea',@Root+N'dish23.jpg'),
(N'Cà phê sữa đá',@Root+N'dish24.jpg'),
(N'Milk Coffee',@Root+N'dish24.jpg'),
(N'Nước khoáng',@Root+N'dish25.jpg'),
(N'Mineral Water',@Root+N'dish25.jpg'),
(N'Bánh phô mai',@Root+N'dish26.jpg'),
(N'Cheesecake',@Root+N'dish26.jpg'),
(N'Bánh sô-cô-la',@Root+N'dish27.jpg'),
(N'Chocolate Cake',@Root+N'dish27.jpg'),
(N'Kem vani',@Root+N'dish28.jpg'),
(N'Ice Cream',@Root+N'dish28.jpg'),
(N'Trái cây theo mùa',@Root+N'dish29.jpg'),
(N'Fruit Salad',@Root+N'dish29.jpg'),
(N'Bánh tiramisu',@Root+N'dish30.jpg'),
(N'Tiramisu',@Root+N'dish30.jpg');
DECLARE @ComboImages TABLE (Name nvarchar(150) PRIMARY KEY,ImageUrl nvarchar(500));
INSERT INTO @ComboImages VALUES
(N'Suất ăn gia đình',@Root+N'combo01.jpg'),
(N'Family Combo',@Root+N'combo01.jpg'),
(N'Suất gia đình (mẫu Yamy)',@Root+N'combo01.jpg'),
(N'Suất gà và nước uống',@Root+N'combo03.jpg'),
(N'Chicken Meal Combo',@Root+N'combo03.jpg'),
(N'Suất gà (mẫu Yamy)',@Root+N'combo03.jpg'),
(N'Suất tráng miệng',@Root+N'combo01.jpg'),
(N'Dessert Combo',@Root+N'combo01.jpg'),
(N'Suất tráng miệng (mẫu Yamy)',@Root+N'combo01.jpg');
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
    WHERE i.IsPrimary=1 AND ( @ReplaceExistingExternal=1 OR NULLIF(LTRIM(RTRIM(i.ImageUrl)),N'') IS NULL
        OR EXISTS (SELECT 1 FROM @Legacy l WHERE l.LocalUrl=REPLACE(i.ImageUrl,N'~/',N'/')) );

    -- Món chưa có ảnh: thêm một ảnh đại diện, không tạo trùng khi chạy lại.
    INSERT INTO dbo.DishImages (DishId,ImageUrl,IsPrimary)
    SELECT d.Id,m.ImageUrl,1 FROM dbo.Dishes d JOIN @DishImages m ON m.Name=d.Name
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DishImages i WHERE i.DishId=d.Id);

    -- Giữ ảnh bổ sung nhưng chuyển các đường dẫn mẫu bị gỡ sang HTTPS.
    UPDATE i SET ImageUrl=l.RemoteUrl FROM dbo.DishImages i JOIN @Legacy l ON l.LocalUrl=REPLACE(i.ImageUrl,N'~/',N'/');

    UPDATE c SET ImageUrl=m.ImageUrl FROM dbo.Comboes c JOIN @ComboImages m ON m.Name=c.Name
    WHERE @ReplaceExistingExternal=1 OR NULLIF(LTRIM(RTRIM(c.ImageUrl)),N'') IS NULL
        OR EXISTS (SELECT 1 FROM @Legacy l WHERE l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/'));
    UPDATE c SET ImageUrl=l.RemoteUrl FROM dbo.Comboes c JOIN @Legacy l ON l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/');

    -- Một số dữ liệu mẫu dùng chung ảnh món cho danh mục: chỉ đổi URL mẫu này.
    UPDATE c SET ImageUrl=l.RemoteUrl FROM dbo.Categories c JOIN @Legacy l ON l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/');
    COMMIT TRANSACTION;
    PRINT N'Đã cập nhật URL ảnh. Giá, tài khoản, đơn và schema được giữ nguyên.';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

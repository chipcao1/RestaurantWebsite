# Dữ liệu mẫu bằng SQL

Mở [seed-demo.sql](seed-demo.sql) bằng SQL Server Management Studio, chọn đúng database của ứng dụng đã chạy `Update-Database`, rồi bấm **Execute**. File dùng schema main, gồm bảng `dbo.Comboes`; không cần bật biến môi trường seed của EF6.

Script không tạo/đổi/xóa bảng, không sửa migration, không ghi đè bản ghi đang có. Các lần chạy đồng thời của script được tuần tự hóa bằng application lock; toàn bộ dữ liệu mới được thêm trong một transaction. Chạy lại với các khóa nhận diện mẫu còn nguyên sẽ không thêm trùng. Không đổi email, tên hoặc mã nhận diện mẫu nếu muốn giữ khả năng này.

Trên database trống, script tạo:

| Dữ liệu | Số lượng |
|---|---:|
| Tài khoản | 4: quản trị, nhân viên, 2 khách hàng |
| Danh mục / món / ảnh món | 6 / 30 / 30 |
| Bàn | 10, mã YM01–YM10 |
| Suất ăn / thành phần | 3 / 10 |
| Khuyến mãi | 5, mã YM10, YM50K, YM15, YM20, YM30K |
| Đặt bàn | 4 |
| Đơn / chi tiết món / thanh toán | 8 / 9 / 8 |
| Khuyến mãi của đơn / đánh giá | 1 / 2 |

Đơn có đủ các trạng thái chờ xác nhận, xác nhận, chế biến, sẵn sàng, hoàn tất và hủy; thanh toán có trạng thái chờ, đã thu, không thành công và đã hoàn. Tổng tiền lấy từ giá món/suất ăn thực tế. Một đánh giá được duyệt, một đánh giá chờ duyệt. Các giao dịch này là dữ liệu minh họa và sẽ xuất hiện trong báo cáo doanh thu.

Mật khẩu chung **chỉ dùng thử nghiệm**: `YamyDemo@2026!`.

| Email | Vai trò |
|---|---|
| admin.yamy@example.test | Quản trị viên |
| staff.yamy@example.test | Nhân viên |
| khach1.yamy@example.test | Khách hàng |
| khach2.yamy@example.test | Khách hàng |

Nếu email đã tồn tại, mật khẩu và trạng thái tài khoản cũ được giữ nguyên; nếu vai trò khác vai trò mẫu thì script dừng và rollback. Danh mục, món, bàn và khuyến mãi cùng khóa đã tồn tại cũng được giữ nguyên. Vì vậy số lượng thêm mới có thể thấp hơn bảng trên.

Cú pháp được kiểm tra bằng Microsoft ScriptDom chế độ SQL Server 2012; cả bốn hash được kiểm chứng bằng `PasswordHasher` của ứng dụng. Các cột INSERT được đối chiếu với migration main. URL ảnh mới được chọn theo tên món, không đóng gói ảnh món/suất ăn trong Yamy. WindowsChecks có kiểm tra thực thi script, tính đúng tổng tiền, đăng nhập mẫu, chạy hai lần không trùng và giữ giá/mật khẩu đã sửa trong database thử nghiệm riêng. Bộ SQL/LocalDB này chưa được thực thi trong môi trường Linux.


## Cập nhật ảnh trên database đang dùng

Mở [update-food-image-urls.sql](update-food-image-urls.sql), chọn đúng database trong SSMS rồi Execute. Script chỉ cập nhật URL ảnh trong DishImages/Comboes và URL danh mục đang trỏ tới ảnh mẫu cũ; món chưa có ảnh được thêm một ảnh đại diện. Không đổi cấu trúc bảng, giá, tên, tài khoản, đơn hay thanh toán.

Ảnh mới được chọn theo tên từng món và ghim commit nguồn. [food-image-catalog.json](food-image-catalog.json) ghi tên Việt/Anh, URL, nguồn và nhận xét đối chiếu ảnh. Các URL đã trả HTTP 200 với nội dung ảnh tại thời điểm kiểm tra. Không tải ảnh vào repository. Một số ảnh nguồn có watermark hoặc độ phân giải thấp; quyền sử dụng từng ảnh chưa được nguồn xác nhận rõ trong metadata.

Combo mặc định trên giao diện hiển thị ảnh các món thành phần đang có trong combo. Cột ImageUrl dùng ảnh món chính làm ảnh dự phòng; suất tráng miệng dùng bánh phô mai. URL HTTPS tùy chỉnh của combo vẫn hiển thị nguyên ảnh bạn nhập.

Script mặc định thay cả đường dẫn mẫu cục bộ và URL raw GitHub cũ của RestaurantWebsite (bản trước dùng ảnh không đúng món), kể cả ảnh bổ sung của các món nhận diện được. Ảnh HTTPS tùy chỉnh khác được giữ nguyên. Đổi `@ReplaceExistingExternal` từ **0** thành **1** nếu muốn thay cả ảnh tùy chỉnh của những món/suất ăn có trong bảng ánh xạ. Có tên Việt, tên Anh của main và tên suất ăn mẫu Yamy. Ảnh không tải được sẽ dùng SVG giữ chỗ.

Script có transaction và chạy lại không thêm trùng ảnh. Schema/migration không thay đổi, không cần Update-Database. File SQL chưa được thực thi trên SQL Server trong môi trường Linux; WindowsChecks có bài kiểm tra trên database thử nghiệm riêng.

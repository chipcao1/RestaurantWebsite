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

Cú pháp được kiểm tra bằng Microsoft ScriptDom chế độ SQL Server 2012; cả bốn hash được kiểm chứng bằng `PasswordHasher` của ứng dụng. Các cột INSERT được đối chiếu với migration main. URL ảnh trỏ tới ảnh có trong commit main cố định, không đóng gói ảnh món/suất ăn trong Yamy. WindowsChecks có kiểm tra thực thi script, tính đúng tổng tiền, đăng nhập mẫu, chạy hai lần không trùng và giữ giá/mật khẩu đã sửa trong database thử nghiệm riêng. Bộ SQL/LocalDB này chưa được thực thi trong môi trường Linux.


## Cập nhật ảnh trên database đang dùng

Mở [update-food-image-urls.sql](update-food-image-urls.sql), chọn đúng database trong SSMS rồi Execute. Script chỉ cập nhật URL ảnh trong DishImages/Comboes và URL danh mục đang trỏ tới ảnh mẫu cũ; món chưa có ảnh được thêm một ảnh đại diện. Không đổi cấu trúc bảng, giá, tên, tài khoản, đơn hay thanh toán.

URL mặc định dùng ảnh gốc đang có tại commit main `2447b85b446d447c5bd7c051271d5dcccce7b7f9` trên GitHub (raw.githubusercontent.com). Không tải bản sao ảnh về dự án. Các file nguồn đã được xác nhận tồn tại trong Git; môi trường hiện tại chặn truy cập HTTP máy chủ ảnh, nên chưa xác nhận tải ảnh qua internet từ môi trường này. Trình duyệt cần truy cập được URL; nếu không web dùng SVG giữ chỗ.

Để dùng ảnh internet mới bạn chọn, sửa các URL trong hai bảng ánh xạ `@DishImages` và `@ComboImages` trong SQL. Có sẵn tên món tiếng Việt, tên tiếng Anh của main và tên suất ăn mẫu Yamy. Dùng URL HTTPS trực tiếp tới ảnh. Đổi `@ReplaceExistingExternal` từ **0** thành **1** khi muốn thay cả URL HTTPS đã có theo bảng ánh xạ. Mặc định **0** giữ ảnh HTTPS tùy chỉnh; chỉ đổi ảnh trống hoặc đường dẫn mẫu cục bộ.

Script có transaction và chạy lại không thêm trùng ảnh. Schema/migration không thay đổi, không cần Update-Database. File SQL chưa được thực thi trên SQL Server trong môi trường Linux; WindowsChecks có bài kiểm tra trên database thử nghiệm riêng.

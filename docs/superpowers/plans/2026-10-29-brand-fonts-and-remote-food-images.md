# Font Feane, logo, Home rút gọn và ảnh món qua HTTPS

Yêu cầu mới thay thế việc Home hiển thị toàn bộ: Home xem trước tối đa 6 kết quả mỗi bộ lọc/tìm kiếm, Menu vẫn đủ dữ liệu. Bỏ dòng đếm món/suất ăn trên cả hai. Giữ schema main.

- [x] Kiểm tra hồi quy preview giới hạn 6 nhưng lọc trên toàn bộ dữ liệu, Menu không giới hạn và không cần DOM hiển thị số lượng.
- [x] Font tiêu đề Dancing Script như Feane, nội dung Open Sans; đóng gói subset latin/vietnamese và license OFL để font hoạt động độc lập Google Fonts. Thêm logo SVG nhẹ, dùng ở đầu trang/public và vận hành/favicon.
- [x] Helper ảnh nhận URL HTTPS và chuyển đường dẫn ảnh mẫu bị gỡ sang URL immutable trên GitHub của bản main. Giữ ảnh nền/ảnh trang giới thiệu và ảnh không thuộc dish/combo; fallback SVG nhẹ khi mạng hoặc ảnh lỗi.
- [x] Loại 43 ảnh dish/combo/f/o khỏi working tree và csproj, sửa mọi tham chiếu trong view, seed và SQL seed. Không viết lại git history.
- [x] Thêm database/update-food-image-urls.sql ánh xạ tên Việt/Anh/mẫu đến URL ảnh, cập nhật riêng URL của DishImages/Comboes và ảnh danh mục mẫu có dùng file bị gỡ. Giữ ảnh HTTPS tùy chỉnh mặc định; có cờ thay thế khi người dùng cung cấp URL mới. Không sửa bảng/cột/migration và không chạy SQL trên DB người dùng.
- [x] Chạy unit/Node, Debug/Release, 60 Razor, snapshot main, phân tích T-SQL và kiểm tra markup/font/logo/preview trong Chromium; biên dịch WindowsChecks, ghi rõ chưa chạy IIS/SQL. Rà soát rồi commit/push Yamy.

## Kết quả

Đã chạy: 70 domain checks, 4 bộ Node, Debug/Release, parse/compile 60 Razor views và so mô hình với main; XML/MIME và manifest font/ảnh cũng được kiểm tra. WindowsChecks biên dịch net48 với 0 lỗi/cảnh báo, chưa thực thi LocalDB. SQL seed và cập nhật URL đều qua ScriptDom SQL Server2012; không chạy trên database người dùng.

Chromium đã kiểm tra Home/Menu/Admin/Combos/Staff tại 390/768/1440px bằng actual generated Razor và helpers/models mẫu: font cục bộ có ký tự Việt tải thành công, logo hợp lệ, Home6/Menu đầy đủ, không có số đếm và không tràn ngang. Kiểm tra ảnh mất mạng ban đầu/modal đều dùng SVG. Ảnh online trong kiểm tra hiển thị được mô phỏng bằng blob ảnh gốc từ git; HTTP máy chủ ảnh bị chặn trong môi trường nên không tuyên bố đã xác minh truy cập internet thật.

43 file món/suất ăn đã gỡ: 4.989.870 byte. Giữ ảnh nền, giới thiệu, favicon/ảnh khách. Font thêm khoảng184KiB. Không viết lại git history; mức giảm áp dụng thư mục checkout/publish. Reviewer độc lập không có phát hiện lỗi xác nhận.

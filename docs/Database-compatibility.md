# Giữ database tương thích với main

Theo yêu cầu ngày 29/10/2026, Yamy sử dụng nguyên schema của main tại commit `2447b85`. Ba file `202609281435011_InitialCreate` không thay đổi. Migration `202610280900000_YamyWorkflow` đã bị loại bỏ, cùng các thuộc tính EF phụ thuộc sáu cột mới. Không thêm migration thay thế hoặc SQL sửa dữ liệu.

Lỗi `Invalid object name 'dbo.Combos'` xuất phát từ migration YamyWorkflow cũ: main tạo bảng `dbo.Comboes`, trong khi câu SQL dịch dữ liệu dùng nhầm `dbo.Combos`. Bản sửa không đổi tên bảng main.

## Khi đã gặp đúng lỗi trên

1. Pull nhánh Yamy mới nhất. Trong Visual Studio chọn Clean Solution, rồi Rebuild Solution để loại migration cũ khỏi DLL. Không dùng DLL cũ hoặc chạy Add-Migration để tạo lại YamyWorkflow.
2. Giữ connection string đang dùng. Không xóa database, bảng, dữ liệu hoặc `__MigrationHistory`.
3. Trong Package Manager Console chọn project RestaurantWebsite và chạy `Get-Migrations`. Mã nguồn chỉ còn `202609281435011_InitialCreate`.
4. Nếu cần, chạy `Update-Database -Verbose` với cấu hình seed minh họa tắt (mặc định). Nếu database đã áp dụng InitialCreate thì không còn migration bổ sung để chạy. Với database mới, chỉ InitialCreate tạo schema như main.

Theo log lỗi, InitialCreate chạy trước, rồi YamyWorkflow thất bại trước bước thêm cột. EF6 thông thường rollback các câu lệnh của migration thất bại trong cùng transaction. Để kiểm tra trạng thái thực tế, có thể chạy truy vấn chỉ đọc trên đúng database:

```sql
SELECT MigrationId, ContextKey
FROM dbo.__MigrationHistory
ORDER BY MigrationId;

SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND COLUMN_NAME IN ('ServiceStart', 'ServiceEnd', 'AssignedStaffId',
                      'RefundedAt', 'ConfirmedById', 'RefundedById');
```

Database giống main chỉ có InitialCreate cho context `RestaurantWebsite.Models.AppDbContext` và không có sáu cột trên. Nếu YamyWorkflow đã từng chạy thành công ở database khác, hoặc có chỉnh SQL thủ công, bản sửa này không tự ý xóa cột, sửa lịch sử migration hay rollback database đó. Cần đối chiếu trạng thái và sao lưu trước khi xử lý riêng.

## Giới hạn khi dùng schema main

- Nhân viên và quản trị xử lý đặt bàn theo quyền vai trò; không có phân công từng nhân viên vì main không lưu trường này.
- Khoảng giữ bàn hai giờ lấy từ đặt bàn liên kết, hoặc thời điểm tạo đơn tại bàn. Đơn đang mở quá giờ vẫn giữ bàn cho đến khi xử lý xong.
- Thu tiền cập nhật PaymentDate như luồng hiện có. Hoàn tiền giữ nguyên PaymentDate và chuyển trạng thái Refunded; không lưu người thu/hoàn hoặc ngày hoàn riêng.
- Báo cáo phản ánh trạng thái thanh toán hiện tại theo ngày thanh toán gốc. Khoản hoàn điều chỉnh kỳ thanh toán gốc; báo cáo không phải nhật ký dòng tiền theo ngày hoàn thực tế. Nội dung này hiển thị trên trang báo cáo và PDF.
- Không dịch hoặc ghi đè tên/mô tả danh mục hiện có bằng migration. Nội dung dữ liệu do người dùng quản lý được giữ nguyên.

## Kiểm chứng

`scripts/check-model-snapshot.py` so sánh mô hình EF6 với snapshot InitialCreate gốc, gồm mô hình dữ liệu, schema lưu trữ và mapping. Linux validation và Windows workflow đều chạy kiểm tra này. WindowsChecks kiểm tra database tạo bằng InitialCreate, tên bảng Comboes, model tương thích và chạy Update-Database lần nữa vẫn giữ nội dung danh mục. Các kiểm tra SQL thật chỉ chạy trên Windows/LocalDB.

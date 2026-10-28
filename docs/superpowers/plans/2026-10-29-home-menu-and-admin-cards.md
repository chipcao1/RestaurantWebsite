# Sửa thực đơn Home, quyền xóa và giao diện vận hành

Yêu cầu: Home lọc ngay tại chỗ và đủ món như customer/Menu; Staff không có xóa món; Combos và tổng quan theo phong cách trang main. Giữ MVC5 và schema InitialCreate.

- [x] Tái hiện lỗi Home chỉ lấy 6 món và Menu bị giới hạn dữ liệu theo categoryId. Viết kiểm tra lọc tại chỗ, trở về Tất cả, tìm kiếm và hai component độc lập.
- [x] Dùng MenuCatalogService + view model lấy toàn bộ món hợp lệ và suất ăn hợp lệ cho Home, CustomerHome và Menu; partial/JavaScript dùng chung. CategoryId chỉ là bộ lọc ban đầu, không làm mất món khỏi dữ liệu HTML. Chi tiết mở modal tại chỗ, không chuyển sang trang tìm kiếm của Menu.
- [x] Chuyển action Delete ra khỏi controller dùng chung, chỉ expose ở Admin; Staff không có action công khai và không có nút xóa (kể cả admin đang dùng Staff).
- [x] Combos có header, ô thống kê, bộ lọc, card/table, ảnh, thành phần, trạng thái/lịch bán, action đồng bộ các trang khác. Form thêm/sửa dùng card và hai cột.
- [x] Tổng quan Admin khôi phục callout, small-box, info-box, bố cục chart + bảng theo main; Việt hóa và tiền đồng. Giữ cách tính tiền từ Payment hiện có, không đưa tổng đơn chưa thu thành doanh thu. Tổng quan Staff dùng callout/small-box/card như main.
- [x] Kiểm chứng Node, build Debug/Release, Razor, schema; thêm WindowsChecks cho số món hơn 6, category không cắt dữ liệu và không có Staff.Delete. Chromium kiểm tra fixture với markup/assets thật ở 390/768/1440px; phân biệt fixture với ứng dụng MVC trên IIS.
- [x] Rà soát độc lập, sửa phát hiện, commit/push Yamy.

## Kết quả kiểm chứng

65 kiểm tra C#, ba bộ Node, Debug/Release, parse/compile 60 Razor view và so model với InitialCreate đều đạt. Đã quan sát lỗi hồi quy trước sửa: thiếu shared filter, thiếu API model/status và chi tiết chưa có cache no-store; sau sửa đạt, gồm kiểm tra phản hồi modal cũ không ghi đè món mới. Hai lỗi cú pháp Razor phát hiện khi kiểm tra đã được sửa và chạy lại thành công.

WindowsChecks đã biên dịch net48 0 lỗi/cảnh báo, thêm kiểm tra ba controller dùng chung dữ liệu đủ món (>6), bộ lọc ban đầu không cắt dataset và Staff không expose Delete. Chưa thực thi trên Windows/LocalDB.

Chromium chạy actual generated Razor templates với model/helpers thử nghiệm trong .NET8, dùng layout/assets thực; cả Home, Menu, Dashboard Admin/Staff và Combos đạt 390/768/1440px. Có kiểm tra không tràn ngang, desktop content không bị sidebar che, filter/tìm kiếm không điều hướng, All sau category ban đầu khôi phục đủ dữ liệu, modal hoạt động. Endpoint chi tiết trong bài kiểm tra trình duyệt được mô phỏng. Đây không phải chạy MVC/IIS thật.

Reviewer độc lập không xác nhận lỗi nghiêm trọng hoặc lỗi cần sửa thêm. Không thay đổi migration/model EF, không chạy SQL trên database người dùng.

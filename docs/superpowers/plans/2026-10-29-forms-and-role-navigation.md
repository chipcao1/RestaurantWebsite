# Sửa form, điều hướng vai trò và giao diện theo main

> **For agentic workers:** Implement inline with superpowers:executing-plans; preserve the user's concrete requirements below.

**Goal:** Đăng nhập về đúng khu vực, admin chuyển khu vực được, giao diện theo main, báo giá mã giảm ngay và giới hạn thao tác tài khoản.

**Architecture:** Giữ MVC5/EF6 và schema InitialCreate nguyên bản. Bố cục khách dùng Feane, vận hành dùng AdminLTE như main; vùng route quyết định menu, Role quyết định quyền. Checkout preview và đặt đơn dùng chung quy tắc tính giá máy chủ, không tăng lượt dùng mã ở preview.

**Tech Stack:** ASP.NET MVC5/net48, EF6, Bootstrap4/AdminLTE3, JavaScript thuần.

**Spec:** Yêu cầu người dùng ngày 29/10/2026 trong hội thoại, gồm năm nhóm sửa cụ thể. Áp dụng trực tiếp trong nhánh Yamy đã được người dùng chỉ định.

## Global Constraints

- Không thêm/sửa migration, model EF hoặc cấu trúc database.
- Đăng nhập Admin/Staff/Customer luôn về AdminHome/StaffHome/CustomerHome; returnUrl không ghi đè điểm đến theo vai trò.
- Admin giữ Role=Admin khi qua Staff; Staff/Customer không được vào Admin.
- Reset về đúng `123456789`, lưu bằng PasswordHasher; xóa endpoint Edit cho thông tin người khác. Không tự khóa tài khoản đang đăng nhập hoặc quản trị cuối.
- Dùng tiếng Việt; phục hồi bố cục/màu sắc main thay vì thiết kế mới.

## Review Focus

- ReturnUrl tới sai khu vực hoặc URL ngoài không vượt qua điều hướng role.
- Admin trong Staff phải nhìn menu Staff và có lối trở về Admin.
- Mã sai/hết lượt hoặc phản hồi cũ đến trễ không hiển thị giảm giá cũ.
- Preview không tin subtotal phía client và không tiêu thụ mã; Place kiểm tra lại trong transaction.
- Gửi POST Edit hoặc newPassword tùy ý không thể sửa hồ sơ/đổi mật khẩu reset theo giá trị client.

## Tasks

- [x] Viết kiểm tra quy tắc khuyến mãi (đúng/sai, phần trăm/cố định, trần, hạn dùng/lượt) và kiểm tra JS phản hồi đến muộn, lỗi mạng, xóa mã.
- [x] Tách PromotionRules + CheckoutQuote để Quote/Place dùng chung. Thêm POST PreviewPromotion được bảo vệ role+antiforgery; cập nhật tóm tắt và hiển thị mã, mức giảm, số tiền thực giảm ngay khi nhập/đổi mã.
- [x] Đăng nhập chuyển thẳng theo role; phục hồi OperationsLayout dựa AdminLTE main, dựa route area để chọn menu và thêm switch cho Admin.
- [x] Phục hồi hero theo slider_section main, loại CSS toàn cục đổi thương hiệu/bố cục; font tiếng Việt, kiểm tra màn hình 390/768/1440px bằng Chromium trên fixture lấy từ view/assets.
- [x] Xóa bộ chọn số lượng ở nhánh khách chưa đăng nhập trong chi tiết món; chỉ Customer đặt món. Giữ nút đăng nhập hoặc lối về khu làm việc tương ứng.
- [x] Xóa Edit API, đổi Details sang chỉ đọc; reset cố định, khóa/mở và CreateStaff được giữ. Kiểm tra Windows controller với returnUrl, reset và Role nhân viên chống giả mạo.
- [x] Chạy Debug/Release, 59 Razor, domain/JS, so model với main, biên dịch WindowsChecks. Rà soát độc lập, commit/push Yamy. Ghi rõ chưa chạy MVC/IIS/LocalDB trong Linux.

## Kết quả

Đã chạy 56 domain checks, hồi quy frontend/checkout (CSRF, mã sai/xóa/mất mạng, phản hồi cũ đến muộn), Debug/Release và 59 Razor view. Mô hình EF6 khớp InitialCreate main. WindowsChecks đích net48 biên dịch 0 lỗi/cảnh báo; chưa thực thi trên Windows. Chromium kiểm tra fixture dùng layout/assets ở 390/768/1440px, không tràn ngang, chữ tiếng Việt và sidebar đạt. Không coi fixture là ứng dụng MVC thật. Rà soát độc lập không có phát hiện lỗi xác nhận.

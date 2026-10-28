# Kết quả kiểm chứng nhánh Yamy

Các thay đổi được thực hiện trên nhánh Yamy, giữ MVC5/.NET Framework 4.8. Môi trường kiểm tra hiện tại là Debian Linux; chưa chạy IIS Express hoặc SQL Server LocalDB.

## Đã chạy thành công

Lệnh tái lập:

```bash
YAMY_DOTNET=/workspace/toolchains/dotnet/dotnet YAMY_VALIDATION_ROOT=/workspace/toolchains bash scripts/validate-linux.sh
```

- Restore các phiên bản NuGet từ packages.config, thêm PDFsharp 1.50.5147 (MIT).
- Build ứng dụng đích net48 ở Debug và Release bằng reference assemblies chính thức.
- 44 kiểm thử C# chạy thành công: quyền chuyển trạng thái, chặn hoàn tất chưa trả tiền, trạng thái cuối, điều kiện hủy đặt bàn, nhãn tiếng Việt, giảm giá/cap/số tiền lẻ, giao khoảng thời gian, URL ảnh, điều kiện suất ăn, thu/hoàn theo ngày thanh toán gốc, số thập phân từ HTML.
- Kiểm thử JavaScript hồi quy chạy thành công khi thiếu phần tử năm trong footer và plugin tùy chọn.
- Parse và biên dịch C# sinh từ 59 Razor view, không có lỗi.
- Mô hình EF6 khớp snapshot InitialCreate của main; ba file InitialCreate được giữ nguyên. Không còn migration YamyWorkflow hoặc sáu cột bổ sung.
- Bộ WindowsChecks đích net48 được biên dịch ở Release, 0 lỗi, 0 cảnh báo.
- `git diff --check`, kiểm tra cú pháp JavaScript, shell và Python đã chạy.

Công cụ RazorCheck phát cảnh báo NU1701 vì chạy parser Razor MVC5 net45 trong tiến trình net8 của công cụ. Mã Razor sinh ra được biên dịch đích net48; cảnh báo này không có nghĩa website đã chạy trên .NET8.

## Đã rà soát

Một tác nhân độc lập đã xem thay đổi theo thiết kế. Không có lỗi Critical/Important được xác nhận trong các luồng chính được rà soát. Ba phát hiện đã được sửa: nút chuyển trạng thái trên đơn đã kết thúc, nội dung tiếng Anh trong catalog và làm tròn giảm giá vượt giới hạn số tiền lẻ. Kiểm thử làm tròn đã tái hiện FAIL trước khi sửa và PASS sau khi sửa. Kiểm thử JavaScript cũng ghi nhận FAIL trước sửa và PASS sau sửa.

## Chưa chạy — cần Windows

- Thực thi migration và kiểm tra truy vấn/giao dịch thật trên LocalDB.
- Thực thi WindowsChecks: route, quyền từ DB, tài khoản khóa, hai checkout tranh cùng bàn hoặc lượt khuyến mãi cuối, thu tiền trùng, đánh giá và PDF nhiều trang/Unicode.
- Khởi động IIS Express, gửi yêu cầu HTTP đại diện và kiểm thử giao diện tương tác trên máy tính/điện thoại.
- Mở và kiểm tra hiển thị dấu tiếng Việt trong PDF bằng font Windows.

Nhánh đã được push. Chưa xác nhận kết quả GitHub Actions từ môi trường hiện tại. Không kết nối hoặc thay đổi database của người dùng. Lần kiểm chứng này không thực thi SQL Server; các kiểm tra tương thích database và giữ dữ liệu trong WindowsChecks cần được chạy trên Windows. Xem [hướng dẫn thực thi và nghiệm thu Windows](Windows-development.md).

## Điều chỉnh theo yêu cầu giữ database main (29/10/2026)

Bỏ migration YamyWorkflow; dùng nguyên schema main và thêm kiểm tra so sánh toàn bộ CSDL/SSDL/mapping EF6 với InitialCreate. Công cụ tạo mô hình lấy tên entity set từ DbSet để phản ánh đúng Combos/Comboes; chỉ bỏ qua danh tính assembly của công cụ khi so sánh, vẫn kiểm tra bảng, cột, khóa và mapping. Bộ WindowsChecks có kiểm tra chỉ tồn tại InitialCreate, model tương thích database gốc, bảng Comboes và chạy lại Update-Database không dịch dữ liệu danh mục. Chưa thực thi bộ WindowsChecks trong Linux.

Bản sửa tương thích main đã được rà soát độc lập: không có phát hiện lỗi được xác nhận. Build Debug/Release, 59 Razor view, 44 kiểm tra logic, kiểm tra frontend và so mô hình với InitialCreate đều đạt. WindowsChecks đã biên dịch Release với 0 lỗi/cảnh báo; chưa thực thi LocalDB/Windows.

## Sửa form và giao diện theo yêu cầu ngày 29/10/2026

- Đăng nhập luôn về trang chủ theo vai trò, gồm trường hợp có returnUrl; tài khoản đang đăng nhập mở Login cũng về đúng khu vực.
- Admin dùng menu theo area đang xem, có nút chuyển Admin/Staff và vẫn giữ quyền Admin. Nhân viên không được vào Admin.
- Khôi phục khung Feane/AdminLTE và bảng màu của main, loại phần CSS đổi giao diện toàn cục, dùng font hỗ trợ tiếng Việt và lớp phủ nền giúp đọc chữ trên ảnh.
- Chi tiết món không có bộ chọn số lượng cho khách chưa đăng nhập; nhân viên/quản trị có lối về khu làm việc.
- Checkout gọi POST PreviewPromotion có CSRF/role, tính giá từ giỏ hàng trong session và database; chia sẻ PromotionRules/Quote với Place. Preview không tiêu thụ lượt. Phản hồi cũ không ghi đè mã mới; xóa mã trả về giá không giảm. Hiển thị mã, mức giảm phần trăm/cố định, trần giảm và số tiền thực giảm. Hỗ trợ cả FixedAmount trong seed main và Fixed Amount.
- Bỏ Users.Edit; trang tài khoản chỉ đọc. Reset cố định 123456789 qua PBKDF2; giữ khóa/mở (không tự khóa hoặc khóa admin cuối), tạo nhân viên cưỡng chế Role=Staff.

Kiểm chứng: 56 kiểm tra logic, frontend/checkout Node, Debug/Release, 59 Razor view và so schema main đều đạt. WindowsChecks có kiểm tra controller thật cho điều hướng, reset, tạo nhân viên và quote; biên dịch net48 0 lỗi/cảnh báo nhưng chưa chạy Windows/LocalDB. Chromium dùng fixture HTML từ layout/assets tại 390, 768, 1440px, xác minh chữ Việt, không tràn ngang và sidebar mở/đóng. Fixture không thay thế rendering MVC thật. Một reviewer độc lập không xác nhận thêm lỗi. Không thay đổi model EF, migration hoặc database của người dùng.

## Hoàn thiện Home/Menu và các trang vận hành (29/10/2026)

- Home, CustomerHome và Menu dùng chung MenuCatalogService/ViewModel, partial và JavaScript. Lấy toàn bộ món Available thuộc danh mục đang hoạt động và suất ăn thực sự phục vụ được; category/search chỉ ẩn/hiện ban đầu, không cắt dữ liệu máy chủ. Không còn Take(6), chuyển trang khi lọc Home hoặc mở Menu theo tìm kiếm một món khi chọn chi tiết.
- Staff không có public Delete; helper protected NonAction chỉ được gọi từ Admin.Delete có CSRF. Nút xóa chỉ xuất hiện ở Admin, kể cả khi tài khoản Admin chuyển sang Staff.
- Combos Index/Edit theo AdminLTE card, small-box, filter, ảnh/thành phần/lịch/trạng thái và form hai cột. Trạng thái bán xét cả ngày và món thành phần.
- Dashboard quản trị theo main: callout, 4 small-box, 6 info-box, chart theo ngày, phân bố theo giờ/hình thức phục vụ, top món; tiền đồng và tiếng Việt. Biểu đồ SVG cục bộ có bảng số liệu, không phụ thuộc Chart.js CDN. Thu ròng vẫn theo Payment; tổng giá trị đơn theo giờ được ghi rõ có thể chưa thu.
- Tổng quan nhân viên dùng callout, small-box và card/table theo main. Thực đơn không còn phụ thuộc Isotope/CDN; gỡ các script CDN không còn được view dùng.

Đã chạy: 65 kiểm tra logic; ba bộ Node (frontend, checkout, menu gồm cache/race của modal); build Debug/Release; 60 Razor parse/compile; EF model khớp main; git diff --check. WindowsChecks biên dịch net48 0 lỗi/cảnh báo, chưa chạy SQL/Windows. Chromium kiểm tra 5 trang × 3 kích thước 390/768/1440px bằng actual generated Razor với helpers/models thử nghiệm; filter đủ món, tìm kiếm/chi tiết tại chỗ, vị trí sidebar và không tràn ngang đều đạt. Endpoint modal trong trình duyệt được mô phỏng; không tuyên bố đã chạy MVC/IIS/SQL. Rà soát độc lập không có phát hiện lỗi được xác nhận. Schema/migration không đổi.

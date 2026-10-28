# Phát triển và kiểm thử trên Windows

Ứng dụng giữ ASP.NET MVC 5/.NET Framework 4.8, EF6 và SQL Server. Không chạy bằng `dotnet run` như ASP.NET Core.

## Công cụ

- Windows 10/11 hoặc Windows Server, Visual Studio 2022 có ASP.NET/Web build tools, .NET Framework 4.8 Developer Pack và IIS Express.
- SQL Server Express LocalDB với instance `MSSQLLocalDB`; NuGet CLI; .NET SDK 8 cho bộ kiểm thử độc lập; Node.js 18+ để chạy kiểm tra JavaScript.
- Dùng checkout hiện tại trên nhánh Yamy, không cần worktree khác.

Trong Developer PowerShell tại thư mục gốc repository:

```powershell
nuget restore RestaurantWebsite/packages.config -PackagesDirectory packages -NonInteractive
msbuild RestaurantWebsite/RestaurantWebsite.csproj /t:Build /p:Configuration=Debug /m:2
msbuild RestaurantWebsite/RestaurantWebsite.csproj /t:Build /p:Configuration=Release /p:MvcBuildViews=true /m:2
dotnet run --project tests/RestaurantWebsite.Tests
node tests/frontend-smoke.cjs
sqllocaldb start MSSQLLocalDB
dotnet build tests/WindowsChecks/WindowsChecks.csproj -c Release
& tests/WindowsChecks/bin/Release/net48/WindowsChecks.exe
```

WindowsChecks tạo database `RestaurantWebsite_Yamy_Test_<GUID>`, chạy migration, kiểm tra route, session giả quyền, tài khoản khóa, checkout, tiến trình, đánh giá, hai checkout tranh bàn/lượt khuyến mãi cuối và PDF Unicode nhiều trang, rồi xóa đúng database vừa tạo. Không dùng database có sẵn của nhà hàng. Việc bộ kiểm thử được biên dịch trên Linux chưa chứng minh các kiểm tra này đã chạy.

## Database phát triển và migration

`RESTAURANT_CONNECTION_STRING` là tùy chọn cho connection string của tiến trình ứng dụng; khi không đặt sẽ dùng connection `RestaurantWebsite` trong Web.config. Không đưa mật khẩu thật vào repository. Bảo đảm chọn đúng database và sao lưu trước khi áp dụng migration cho dữ liệu đang dùng.

Ví dụ tạo database phát triển riêng bằng Windows authentication:

```powershell
$env:RESTAURANT_CONNECTION_STRING = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RestaurantWebsite_Yamy_Dev;Integrated Security=True;MultipleActiveResultSets=True'
& packages/EntityFramework.6.5.2/tools/net45/any/ef6.exe database update --assembly RestaurantWebsite/bin/RestaurantWebsite.dll --config RestaurantWebsite/Web.config
if ($LASTEXITCODE -ne 0) { throw 'Migration failed' }
```

Yamy giữ nguyên schema của `main` và chỉ dùng migration `202609281435011_InitialCreate`, giữ nguyên cả ba file migration gốc. Không có migration bổ sung, đổi tên bảng, thêm cột hoặc SQL dịch dữ liệu hiện có. Bảng suất ăn vẫn tên `dbo.Comboes` theo main. Database đã có InitialCreate không cần cập nhật schema khi chuyển sang Yamy. Xem [hướng dẫn xử lý lỗi migration cũ](Database-compatibility.md).

Thời gian phục vụ được suy ra từ `ReservationDate` của đặt bàn liên kết hoặc `CreatedAt` của đơn tại bàn. Schema main không có cột phân công nhân viên và nhật ký thu/hoàn riêng, nên giao diện không cung cấp phân công nhân viên. Báo cáo điều chỉnh khoản đã hoàn về ngày thanh toán gốc (`PaymentDate`), không coi đó là ngày hoàn thực tế. DateTime hiện có được coi là giờ địa phương Việt Nam; không chuyển đổi dữ liệu lịch sử.

Seed không tự ghi đè tài khoản khi chạy migration. Chỉ khi tạo database minh họa mới hoàn toàn, đặt `RESTAURANT_SEED_DEMO=true` và cấp `RESTAURANT_DEMO_PASSWORD` ít nhất 12 ký tự trong môi trường tiến trình, rồi chạy migration. Không ghi mật khẩu vào lệnh được lưu lịch sử, tài liệu hoặc commit. Sau khi tạo dữ liệu, xóa hai biến seed khỏi môi trường. Với database thật, quản lý tài khoản theo quy trình riêng của đơn vị.

## Khởi động

Chạy từ Developer PowerShell có cùng biến connection string. Có thể mở csproj trong Visual Studio và chạy IIS Express, hoặc:

```powershell
$sitePath = (Resolve-Path RestaurantWebsite).Path
& "${env:ProgramFiles}\IIS Express\iisexpress.exe" "/path:$sitePath" /port:58814
```

Chờ thông báo IIS Express đã khởi động. Trong cửa sổ PowerShell khác, gửi yêu cầu HTTP nội bộ và xác minh trang chủ chứa thực đơn hoặc trạng thái rỗng tiếng Việt. Sau đó đăng nhập bằng tài khoản phát triển đã tạo và kiểm tra checklist bên dưới. Không coi một tiến trình còn sống hoặc cổng mở là kiểm chứng ứng dụng hoạt động.

## Nghiệm thu thủ công

| Nhóm | Kiểm tra |
|---|---|
| Tài khoản/phân quyền | Đăng ký khách; đăng nhập ba vai trò; đổi mật khẩu; khóa/đổi quyền có hiệu lực ở yêu cầu tiếp theo; POST trái quyền bị chặn; không khóa/hạ quyền quản trị viên cuối. |
| Danh mục/món | Nhân viên và quản trị thêm/sửa món; sửa giá, thời gian chế biến; thêm/gỡ ảnh/chọn đại diện; ẩn dữ liệu đã được tham chiếu thay vì xóa lịch sử. |
| Đặt món | Thêm món và suất ăn; số lượng; đổi gợi ý suất ăn; áp mã đúng/sai/hết lượt; giao hàng cần địa chỉ, mang về không có địa chỉ/phí giao hàng, tại bàn lưu đúng phiên bàn. |
| Đặt bàn | Số khách không vượt sức chứa; xác nhận/hủy/đổi bàn không trùng giờ; nhân viên/quản trị xử lý theo vai trò; nhiều đơn trong một phiên vẫn giữ chỗ khi còn đơn đang mở. |
| Thanh toán/hóa đơn | Ghi nhận thủ công tiền mặt/chuyển khoản/ví; mã giao dịch; chặn thu trùng; chưa trả đủ không hoàn tất; hoàn toàn phần trước chế biến đồng thời hủy đơn. Không tích hợp cổng thanh toán thật. |
| Đánh giá | Đánh giá món trong đơn hoàn tất; chặn đơn người khác, đơn chưa hoàn tất và gửi trùng; quản trị duyệt/ẩn, nhân viên chỉ xem. |
| Báo cáo/PDF | Thu và khoản đã hoàn theo ngày thanh toán gốc; lọc ngày; xuất doanh thu, thực đơn và hóa đơn; mở PDF và kiểm tra dấu tiếng Việt, ngắt trang, đúng số tiền. |
| Giao diện | Kiểm tra trên máy tính/điện thoại; nội dung tiếng Việt; tiền đồng; sidebar đúng vai trò; trạng thái rỗng/lỗi/thành công; thao tác bằng bàn phím. |

## Kiểm tra trên Linux

```bash
YAMY_DOTNET=/path/to/dotnet bash scripts/validate-linux.sh
```

Script khôi phục đúng phiên bản NuGet vào thư mục kiểm chứng, biên dịch Debug/Release với reference assemblies net48, chạy test C# độc lập, parse và compile Razor, so mô hình EF6 với snapshot InitialCreate nguyên bản của main. Nó chỉ bỏ các target quản lý tiến trình VBCSCompiler qua Windows WMI khi chạy MSBuild .NET trên Linux; không tắt bước biên dịch, assertion, TLS hay xác minh package. Warning NU1701 ở công cụ RazorCheck là do dùng parser Razor MVC5 net45 trong tiến trình kiểm chứng net8; mã sinh ra vẫn được biên dịch đích net48.

Không dùng Linux để tuyên bố đã chạy IIS Express, LocalDB hoặc kiểm tra hiển thị PDF bằng font Windows. Workflow `.github/workflows/windows-validation.yml` chuẩn bị các kiểm tra Windows sau khi nhánh được đẩy lên; việc có file workflow không đồng nghĩa CI đã chạy.

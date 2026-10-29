# Chạy và nộp RestaurantWebsite

Yêu cầu Windows, Visual Studio hỗ trợ `.slnx` (VS 2022 17.12 trở lên), workload ASP.NET and web development, .NET Framework 4.8 Developer Pack và SQL Server / SQL Express / LocalDB.

## Cách chạy

1. Mở `RestaurantWebsite.slnx`, chọn **Restore NuGet Packages** nếu thiếu thư viện. Giữ `packages` cùng cấp với solution và thư mục project.
2. Sửa `Data Source` trong connection string `RestaurantWebsite` ở `Web.config` cho đúng SQL Server trên máy. Database mặc định là `RestaurantWebsite`.
3. Chọn project `RestaurantWebsite` làm startup project, build, mở Package Manager Console, chọn Default project `RestaurantWebsite` và chạy:

```powershell
Update-Database
```

Lệnh tự tạo bảng theo migration gốc main và tự thêm dữ liệu mẫu; không cần biến môi trường, không cần chạy SQL seed riêng. Khi chạy lại, dữ liệu mẫu còn nguyên khóa nhận diện không bị thêm trùng; mật khẩu, giá, trạng thái và nội dung bản ghi đang có được giữ. Các URL ảnh mẫu cũ được đổi sang ảnh mới đúng tên món; URL HTTPS tùy chỉnh khác được giữ. Không đổi schema và không chạy Add-Migration.

4. Chạy bằng IIS Express (F5).

## Tài khoản mẫu

Mật khẩu chung: **`YamyDemo@2026!`**.

| Email | Vai trò |
|---|---|
| admin.yamy@example.test | Quản trị viên |
| staff.yamy@example.test | Nhân viên |
| khach1.yamy@example.test | Khách hàng |
| khach2.yamy@example.test | Khách hàng |

Nếu tài khoản mẫu đã tồn tại, mật khẩu cũ được giữ nguyên. Không sử dụng các tài khoản mẫu cho triển khai công khai.

## DATABASE.sql

File `DATABASE.sql` ở cạnh solution gồm cả schema gốc, lịch sử migration EF6 và cùng dữ liệu seed được ứng dụng sử dụng. Mở trong SSMS và chạy toàn bộ để tạo database `RestaurantWebsite` mà không cần chạy Update-Database trước. Đổi tên trong đầu script nếu cần dùng database tên khác, rồi sửa connection string tương ứng.

Chỉ cần một trong hai cách khởi tạo: `Update-Database` hoặc `DATABASE.sql`. Sau khi chạy SQL vẫn có thể chạy Update-Database; migration gốc đã được ghi nhận. Script không xóa database hoặc bảng. Đối với database có bảng nhưng không có lịch sử EF6, cần xác minh nguồn database trước khi chạy script khởi tạo; script không tự nhận một schema chưa được kiểm chứng.

## Thành phần bản nộp

```text
RestaurantWebsite.slnx
RestaurantWebsite/       # mã nguồn, tài nguyên, migrations, seed nhúng và hướng dẫn
packages/                # thư viện NuGet thật; có thể khôi phục bằng NuGet
DATABASE.sql             # khởi tạo độc lập bằng SSMS
```

`Database/Seed.sql` và `Database/UpdateFoodImages.sql` nằm trong project và được nhúng vào DLL khi build; seed không phụ thuộc thư mục ngoài project hay đường dẫn làm việc. Metadata nguồn ảnh ở `Database/food-image-catalog.json`. Ảnh món ăn được tải qua internet, không đóng gói ảnh nặng trong repository.

Không cần nộp `.git`, `.vs`, `bin`, `obj` hoặc các công cụ kiểm thử. Git không lưu `packages`; sau khi clone cần Restore NuGet Packages trước khi đóng gói bản nộp offline.

## Kiểm tra bản này

Đã biên dịch net48 Debug/Release, kiểm tra domain/frontend và biên dịch Razor trên Linux; SQL được kiểm tra cú pháp SQL Server 2012 và model khớp migration main. Chưa thực thi SQL Server/LocalDB hoặc IIS trong môi trường Linux. Nên chạy `Update-Database` trên Windows trước khi nộp.

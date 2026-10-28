# Nhà hàng Feane — RestaurantWebsite

Ứng dụng quản lý nhà hàng bằng ASP.NET MVC 5, .NET Framework 4.8 và Entity Framework 6. Giao diện dùng tiếng Việt; có khu vực quản trị viên, nhân viên và khách hàng.

- `RestaurantWebsite/Areas/Admin`: tài khoản, thực đơn, suất ăn kết hợp, khuyến mãi, bàn, đánh giá và báo cáo.
- `RestaurantWebsite/Areas/Staff`: hàng đợi đơn, thu/hoàn tiền, đặt bàn và quản lý phục vụ.
- `RestaurantWebsite/Areas/Customer`: thực đơn, giỏ hàng, đặt món, đặt bàn và hồ sơ khách hàng.
- `RestaurantWebsite/Services`: quy tắc, giao dịch và báo cáo dùng chung giữa các vai trò.
- `RestaurantWebsite/Controllers/Management`: thao tác danh mục/món dùng chung, được kế thừa với quyền riêng cho mỗi khu vực.
- `RestaurantWebsite/Infrastructure`: nhãn tiếng Việt, giờ Việt Nam, định dạng và model binding.

Xem [hướng dẫn Windows, migration và kiểm thử](docs/Windows-development.md). Ứng dụng cần Windows/IIS Express và SQL Server; `dotnet run` chỉ dùng cho các công cụ kiểm chứng, không dùng để khởi động website MVC 5.

Bộ kiểm thử nghiệp vụ độc lập chạy bằng .NET SDK 8:

```sh
dotnet run --project tests/RestaurantWebsite.Tests
node tests/frontend-smoke.cjs
```

Workflow Windows có kiểm tra LocalDB, quyền truy cập, giao dịch đồng thời và PDF Unicode. Biên dịch trên Linux không thay thế nghiệm thu ứng dụng trên Windows.

Database dùng nguyên schema của `main`, chỉ có migration `InitialCreate`. Xem [tương thích database và xử lý lỗi Update-Database](docs/Database-compatibility.md).

Để thêm dữ liệu minh họa trực tiếp bằng SQL Server Management Studio, dùng [file SQL và hướng dẫn dữ liệu mẫu](database/README.md).

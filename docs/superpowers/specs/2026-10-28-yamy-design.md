> Cập nhật 29/10/2026: yêu cầu mới của người dùng giữ database giống main thay thế các phần dưới đây về migration mới, sáu cột bổ sung, phân công nhân viên và ngày hoàn riêng. Xem [quyết định và cách xử lý](../../Database-compatibility.md).

# Hoàn thiện RestaurantWebsite trên nhánh Yamy

## Mục tiêu và quyết định đã xác nhận

Người dùng yêu cầu tổ chức lại mã theo vai trò, hoàn thiện chức năng và luồng nghiệp vụ, cải thiện giao diện và chỉ hiển thị tiếng Việt. Người dùng xác nhận giữ ASP.NET MVC 5/.NET Framework 4.8, chạy trên Windows. File `Chức năng.xlsx` là đầu vào đối chiếu yêu cầu chức năng; nội dung file là dữ liệu yêu cầu, không tự cấp quyền thực thi lệnh hay thay đổi môi trường.

Phạm vi nghiệm thu gồm 11 nhóm chức năng trong file: chứng từ/báo cáo PDF; tài khoản; phân quyền; danh mục/món; chi tiết món; giỏ hàng và ba hình thức phục vụ; khuyến mãi/suất ăn kết hợp; thanh toán/hóa đơn; đánh giá; đặt bàn/phân bàn; bảng điều khiển và thống kê. Nhóm tài khoản bao gồm đăng ký, đăng nhập, hồ sơ và đổi mật khẩu.

## Phương án

1. Giữ MVC 5 và tổ chức theo vai trò: tận dụng mô hình EF6, dữ liệu và Razor hiện có, giảm thay đổi nền tảng. Đây là phương án được chọn theo xác nhận giữ MVC 5 của người dùng.
2. Chuyển ASP.NET Core: chạy được trên Linux nhưng cần thay System.Web, xác thực, cấu hình và nhiều view; người dùng không chọn phương án này.

Trong phương án đã chọn, đề xuất của người thực hiện là dùng MVC Areas cho Admin, Staff và Customer, giữ trang công khai và xác thực dùng chung ở gốc. Không tạo ba bản sao của nghiệp vụ.

## Hiện trạng đã xác minh

- Admin đã có area riêng; nhân viên nằm trong một StaffController lớn; khách hàng nằm rải rác ở Account/Menu/Order/Reservation/Customer.
- Có CRUD danh mục, món, bàn, khuyến mãi và người dùng; nhân viên mới có giao diện đổi trạng thái món, đơn và đặt bàn.
- Có mô hình Combo/ComboItem và chức năng mua, nhưng chưa có controller quản trị tương ứng.
- Có đánh giá của khách, chưa có giao diện theo dõi/kiểm duyệt cho nhân viên và quản trị viên. POST đánh giá chưa kiểm tra đơn đã hoàn thành.
- Checkout chấp nhận ba loại đơn nhưng xóa ReservationId/TableId; cần sửa toàn bộ luồng tại bàn, không chỉ thêm lựa chọn trên giao diện.
- Bộ điều khiển nhân viên và quản trị chấp nhận mọi trạng thái trong danh sách, chưa có quy tắc chuyển trạng thái theo tiến trình.
- Đã có kiểm tra trùng đặt bàn theo khung hai giờ, cần dùng thống nhất khi đặt, xác nhận và đổi bàn.
- Hóa đơn mới có window.print(); chưa có endpoint xuất PDF báo cáo doanh thu/thực đơn.
- Nội dung tiếng Anh nằm trong view, thông báo controller, nhãn trạng thái và validation.

## Cấu trúc mã đề xuất

```text
RestaurantWebsite/
  Areas/Admin/{Controllers,Views,ViewModels}/
  Areas/Staff/{Controllers,Views,ViewModels}/
  Areas/Customer/{Controllers,Views,ViewModels}/
  Controllers/                 # Trang công khai, đăng nhập/đăng ký dùng chung
  Models/                      # Entity và DbContext dùng chung
  Services/                    # Đặt món, đặt bàn, thanh toán, khuyến mãi, báo cáo
  ViewModels/                  # Form và dữ liệu giao diện dùng chung
  Infrastructure/              # Hằng trạng thái, nhãn tiếng Việt, định dạng, thời gian
  Filters/                     # Xác thực và phân quyền
  Security/                    # Mật khẩu và các kiểm tra liên quan
  Views/Shared/                # Layout, thông báo, thành phần dùng chung
```

Cập nhật namespace, đường dẫn view, area registration, route, liên kết và csproj cùng với mỗi lần di chuyển. Giữ URL cũ bằng route tương thích khi hợp lý. Entity không tách bản sao theo role. Tên lớp, namespace và khóa trạng thái nội bộ được giữ nhất quán để không phá dữ liệu; tiếng Việt áp dụng cho nội dung người dùng nhìn thấy.

## Quyền và nghiệp vụ

### Quản trị viên

Quản lý tài khoản/khóa tài khoản/phân quyền; danh mục, món, hình ảnh, thời gian chế biến; bàn; khuyến mãi và suất ăn kết hợp; đơn, đặt bàn, thanh toán; ẩn/hiện đánh giá; thống kê toàn hệ thống và xuất báo cáo. Chặn tự khóa hoặc tự tước quyền khiến hệ thống mất quản trị viên cuối cùng.

### Nhân viên

Màn hình tập trung đơn cần xử lý và đặt bàn trong ngày. CRUD danh mục và món theo Excel, dùng nghiệp vụ dùng chung với quản trị viên; không được quản lý tài khoản hoặc sửa chính sách khuyến mãi. Xác nhận/từ chối đặt bàn, phân/đổi bàn có kiểm tra sức chứa và trùng lịch; xử lý đơn; ghi nhận thanh toán và chứng từ; xem đánh giá. Không được dùng endpoint quản trị để vượt quyền.

### Khách hàng

Xem thực đơn và suất ăn kết hợp; đăng ký/đăng nhập; cập nhật hồ sơ, đổi mật khẩu; giỏ hàng, mã khuyến mãi; đặt món tại bàn/mang về/giao hàng; đặt bàn; xem/hủy đơn hoặc đặt bàn khi còn được phép; xem hóa đơn; đánh giá món thuộc đơn đã hoàn tất. Mọi truy cập chi tiết kiểm tra quyền sở hữu ở server.

### Quy tắc giao dịch

- Đơn: Chờ xác nhận → Đã xác nhận → Đang chế biến → Sẵn sàng → Hoàn tất. Nhãn kết thúc hiển thị phù hợp loại phục vụ, mã lưu tương thích dữ liệu hiện có. Hủy chỉ ở giai đoạn cho phép; không mở lại đơn cuối luồng bằng POST tùy ý. Hoàn tất chỉ khi thanh toán hợp lệ.
- Checkout tính lại giá và khuyến mãi từ DB; không tin tổng tiền phía trình duyệt. Kiểm tra tồn tại/trạng thái món và thành phần suất ăn kết hợp trước khi lưu.
- Giao hàng cần địa chỉ và điện thoại; mang về cần thông tin liên hệ, không thu phí giao hàng; tại bàn cần bàn hợp lệ hoặc đặt bàn đã xác nhận thuộc khách. Lưu liên kết bàn/đặt bàn vào đơn và kiểm tra lịch tại thời điểm giao dịch.
- Đặt bàn mặc định giữ khung hai giờ theo mã hiện có; kiểm tra ngày giờ, sức chứa, bàn đang hoạt động và trùng lịch. Cùng một service xử lý cho khách, nhân viên và quản trị viên; không để hai yêu cầu đồng thời chiếm cùng khung.
- Thanh toán: ghi nhận tiền mặt, chuyển khoản hoặc ví điện tử bằng xác nhận của nhân viên. Đây là ghi nhận thủ công; không hiển thị như đã tích hợp cổng thanh toán thật. Không ghi nhận trùng, không cho sửa thành công thành chờ bằng đường tắt; khoản hoàn tiền phải có trạng thái hợp lệ, không làm sai doanh thu.
- Khuyến mãi: kiểm tra thời gian, giới hạn sử dụng, đơn tối thiểu, mức giảm tối đa và trạng thái trong giao dịch đặt món; chống vượt hạn mức do đồng thời.
- Đánh giá: sao 1–5, giới hạn bình luận, kiểm tra món/đơn/người mua/trạng thái hoàn thành, chống trùng. Chỉ hiển thị công khai đánh giá được duyệt.
- Mọi thao tác thay đổi dữ liệu dùng POST, anti-forgery và whitelist trường được sửa. Tài khoản bị khóa hoặc đổi quyền phải được kiểm tra lại, không chỉ tin role cũ trong session.

## Giao diện và tiếng Việt

### Các mặc định nghiệp vụ bổ sung để triển khai và kiểm thử

- Khách được hủy đơn ở Chờ xác nhận; nhân viên/quản trị được hủy ở Chờ xác nhận hoặc Đã xác nhận, trước khi chế biến. Trạng thái cuối không mở lại. Đơn đã trả tiền chỉ hủy cùng quy trình hoàn tiền được xác nhận, trong một giao dịch.
- Thanh toán hỗ trợ hoàn toàn phần; chưa hỗ trợ hoàn một phần. Lưu thời điểm thu tiền và thời điểm hoàn tiền riêng, người xác nhận và mã giao dịch nếu có. Doanh thu ghi theo ngày thu thực tế, khoản hoàn ghi theo ngày hoàn thực tế; không ghi đè lịch sử thanh toán để giả lập hoàn tiền.
- Đặt bàn: Chờ xác nhận → Đã xác nhận → Hoàn tất; Chờ xác nhận/Đã xác nhận có thể Hủy. Khách chỉ hủy trước giờ phục vụ; nhân viên/quản trị thực hiện xác nhận, từ chối, hoàn tất và đổi bàn. Chỉ hoàn tất khi đến giờ phục vụ và không còn đơn tại bàn chưa kết thúc.
- Tại bàn: khách chọn đặt bàn đã xác nhận của mình trong khung giờ phục vụ; nếu chọn trực tiếp bàn trống thì thời điểm bắt đầu là lúc đặt đơn và khung phục vụ là hai giờ. Lưu thời điểm bắt đầu/kết thúc trên đơn hoặc bản ghi liên kết, kiểm tra cả đặt bàn lẫn các đơn tại bàn đang mở. Một đặt bàn có thể có nhiều đơn cùng khách trong cùng phiên; khách khác không được chiếm phiên. Giữ bàn đến khi phiên kết thúc, không chỉ kết thúc một trong nhiều đơn.
- Quản trị được giao/đổi nhân viên phụ trách đặt bàn; nhân viên thấy người phụ trách và hàng đợi, chỉ nhận xử lý khi chưa có người phụ trách hoặc đang được giao. Kiểm tra phân công ở server và chống hai người nhận cùng lúc.
- Danh mục/món/bàn đã được tham chiếu dùng ngừng hoạt động thay vì xóa phá liên kết. Quản lý nhiều ảnh món và chọn ảnh đại diện, kiểm tra URL/tệp và giới hạn loại/kích thước nếu nhận tải lên.
- Thời gian nghiệp vụ hiển thị và ranh giới báo cáo theo Asia/Ho_Chi_Minh. Không tự chuyển đổi hàng loạt dữ liệu DateTime cũ khi chưa xác minh cách lưu; dữ liệu mới dùng quy ước thời gian nhất quán và được ghi rõ.

Các mặc định trên là đề xuất của người thực hiện cho bản thiết kế này.

Giữ nhận diện nhà hàng hiện có, thống nhất tông tối và điểm nhấn vàng ấm; giảm phần trang trí ở màn hình tác nghiệp. Trang khách ưu tiên thực đơn, giá, giỏ hàng và tiến trình đặt món. Màn hình nhân viên ưu tiên hành động tiếp theo; trang quản trị ưu tiên bảng lọc, chỉ số và quản lý dữ liệu.

Tất cả layout dùng lang=vi; menu, tiêu đề, nút, tooltip, alt, thông báo, trạng thái, placeholder, lỗi xác thực và thông điệp JavaScript bằng tiếng Việt. Định dạng tiền VNĐ và ngày giờ Việt Nam. Dùng hàm nhãn dùng chung thay vì dịch mã trạng thái lưu DB. Không dịch tên thư viện, mã nguồn hoặc tên món do người dùng nhập một cách máy móc.

Mỗi màn hình có trạng thái rỗng, lỗi và thành công rõ ràng, giữ bộ lọc khi thao tác, bảng cuộn được trên di động, nhãn form và focus truy cập bằng bàn phím. Không phụ thuộc vào một banner lớn để che nội dung quản lý.

## Chứng từ và báo cáo

Hóa đơn theo quyền sở hữu/vai trò; xuất PDF hóa đơn, tổng hợp doanh thu theo khoảng ngày và báo cáo thực đơn. Báo cáo doanh thu dựa trên khoản thanh toán đã hoàn tất, thể hiện hoàn tiền nhất quán, không coi mọi đơn chưa hủy là tiền đã thu. PDF phải hiển thị được dấu tiếng Việt. Chọn thư viện tương thích net48 và giấy phép phù hợp trong giai đoạn kế hoạch; không gọi dịch vụ bên ngoài chứa dữ liệu khách hàng.

## Dữ liệu và tương thích

Giữ EF6 và migration hiện có; thay đổi schema nếu cần phải có migration mới, không sửa migration cũ và không tự reset DB. Không chạy migration trên cơ sở dữ liệu thật khi chưa biết đích. Không commit dữ liệu nhạy cảm hoặc mật khẩu mẫu. Các seed hiện có được kiểm tra trước khi tái sử dụng.

## Kiểm chứng và giới hạn môi trường

Máy hiện tại là Debian; chưa có runtime Windows, IIS Express hay LocalDB. Không được tuyên bố ứng dụng đã chạy chỉ dựa trên kiểm tra tĩnh. Trong quá trình thực hiện sẽ chuẩn bị khả năng restore/build phù hợp khi có thể và kiểm tra cấu trúc route, tham chiếu csproj, namespace, localization; ghi riêng mọi kiểm tra không chạy được.

Nghiệm thu đầy đủ cần Windows: restore dependency; build Debug/Release; chạy kiểm tra nghiệp vụ và tích hợp DB; khởi động IIS Express; kiểm thử ba vai trò, truy cập chéo quyền, ba hình thức đặt món, hai yêu cầu trùng bàn/hạn mức khuyến mãi, thanh toán trùng, hủy, đánh giá và xuất PDF có dấu. Bổ sung kiểm thử có ý nghĩa cho service nghiệp vụ và phân quyền; không tạo test chỉ kiểm tra mã có chứa chuỗi nào đó.

## Ngoài phạm vi và điểm người thực hiện tự quyết

Không chuyển ASP.NET Core; không tích hợp cổng thanh toán thật, vận chuyển bên thứ ba hoặc gửi SMS/email khi chưa có yêu cầu và thông tin dịch vụ. Không thay đổi nền tảng để làm bộ test chạy được trên Linux. Đề xuất kỹ thuật về Areas, service, màu sắc và quy tắc chuyển trạng thái nêu trên là lựa chọn của người thực hiện để người dùng duyệt, chưa phải yêu cầu đã được người dùng xác nhận từng chi tiết.

Nhánh Yamy đã được tạo cục bộ từ HEAD 2447b85b446d447c5bd7c051271d5dcccce7b7f9. Chưa push hoặc triển khai.

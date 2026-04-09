# Tài liệu Chi tiết Hệ thống Quản lý Đoàn viên

Hệ thống **QuanLyDoanVien** là một ứng dụng Full-stack được thiết kế để quản lý thông tin đoàn viên và các đơn vị (Units), tích hợp công cụ tổng hợp báo cáo từ dữ liệu Excel.

---

## 🏗️ Kiến trúc Tổng quát

- **Backend**: ASP.NET Web API 2 (.NET Framework 4.8), Entity Framework Code First.
- **Frontend**: Angular 17+ (SPA).
- **Cơ sở dữ liệu**: SQL Server.

---

## 📂 Backend: QuanLyDoanVien.Web

### 1. Thư mục `Api/` (Các REST Endpoints)

Các Controller này định nghĩa các điểm cuối API mà Frontend gọi đến.

- **`AuthApiController.cs`**:
  - `login`: Đăng nhập, trả về JWT Token và quyền hạn.
  - `logout`: Vô hiệu hóa token hiện tại.
  - `me`: Lấy thông tin người dùng hiện tại đang đăng nhập.
  - `change-password`: Đổi mật khẩu người dùng.
- **`UnitsApiController.cs`** (Quy mô lớn nhất):
  - `GetAll/GetById`: Quản lý danh sách đơn vị.
  - `Create/Update/Delete`: Các chức năng CRUD cơ bản.
  - `import`: **Chức năng quan trọng nhất**. Đọc file Excel, bóc tách dữ liệu đoàn viên, tính toán thống kê (Giới tính, độ tuổi, đảng viên, v.v.) và lưu kết quả dạng JSON vào database.
  - `combined-summary`: Gộp dữ liệu báo cáo từ nhiều đơn vị đã chọn thành một báo cáo tổng hợp duy nhất.
  - `export`: Xuất dữ liệu thống kê ra file Excel.
- **`UsersApiController.cs`**: Quản lý tài khoản người dùng, phân quyền (Roles).
- **`FileApiController.cs`**: Quản lý việc tải lên (Upload) và tải về (Download) các file Excel đính kèm.
- **`RolesApiController.cs`**: Quản lý các vai trò và bộ quyền (Permissions) trong hệ thống.
- **`OtherApiControllers.cs`**: Xử lý các danh mục phụ (Dân tộc, Tôn giáo, Nhóm đoàn viên).

### 2. Thư mục `Models/` (Cấu trúc Dữ liệu)

- **`AppDbContext.cs`**: Cấu hình Entity Framework, kết nối các Entity với các bảng trong SQL Server.
- **`Entities/ModuleEntities.cs`**:
  - `Member`: Thông tin chi tiết của một đoàn viên (Họ tên, ngày sinh, giới tính, thông tin Đảng...).
  - `Unit`: Thông tin đơn vị và quan trọng nhất là trường `SummaryJson` lưu trữ dữ liệu báo cáo đã tổng hợp.
  - `MemberGroup`: Các tổ chức, nhóm đoàn viên.
- **`Entities/SharedEntities.cs`**: Các thực thể dùng chung như `User`, `UserToken`, `Role`, `Permission`, `FileAttachment`, `AuditLog`.

### 3. Thư mục `Services/` (Logic Nghiệp vụ)

- **`AuthService.cs`**: Xử lý logic đăng nhập, tạo token, kiểm tra quyền hạn (Permissions) dựa trên Role.
- **`ExcelService.cs`**: Sử dụng thư viện `EPPlus` để bóc tách file Excel phức tạp (hỗ trợ Header 2 dòng, ô gộp Merged Cells).
- **`FileService.cs`**: Quản lý lưu trữ file vật lý trên server.
- **`AuditService.cs`**: Ghi lại lịch sử thao tác của người dùng (Ai đã làm gì, vào lúc nào).

### 4. Thư mục `Filters/` (Bảo mật)

- **`ApiAuthorizeAttribute.cs`**: Bộ lọc kiểm tra Token và quyền hạn của người dùng trước khi cho phép truy cập API.

---

## 📂 Frontend: QuanLyDoanVien.Angular

### 1. Cấu hình chính

- **`app-routing.module.ts`**: Định nghĩa sơ đồ điều hướng (Routing). Phân chia thành các module: `dashboard`, `users`, `roles`, `members`, `units`, `files`.
- **`app.module.ts`**: Đăng ký các module, service và interceptor của toàn ứng dụng.

### 2. Thư mục `pages/` (Giao diện Chức năng)

Mỗi thư mục thường chứa một Component danh sách (List) và một Component biểu mẫu (Form).

- **`dashboard/`**: Hiển thị biểu đồ thống kê tổng quan toàn hệ thống.
- **`units/`**: 
  - Quản lý đơn vị.
  - Tích hợp giao diện **Import Excel** để tổng hợp dữ liệu.
  - Giao diện **Xem báo cáo** (Hiển thị các bảng số liệu giới tính, độ tuổi...).
- **`members/`**: Quản lý hồ sơ chi tiết của từng đoàn viên.
- **`users/ & roles/`**: Quản lý hệ thống phân quyền.

### 3. Thư mục `core/` (Lõi ứng dụng)

- **`services/`**: Các service gọi API (Sử dụng `HttpClient`) để giao tiếp với Backend.
- **`guards/auth.guard.ts`**: Chặn người dùng chưa đăng nhập tiếp cận các trang quản trị.
- **`interceptors/`**: Tự động chèn Bearer Token vào header của mọi yêu cầu API.

---

## 🔄 Luồng xử lý Quan trọng: Tổng hợp dữ liệu Excel

1. **Frontend**: Người dùng chọn 1 hoặc nhiều file Excel -> Tải lên Backend qua `FileApi`.
2. **Backend (`UnitsApiController.Import`)**: 
   - Nhận ID file -> Gọi `ExcelService` để đọc.
   - `ExcelService` xử lý các cột phức tạp -> Trả về danh sách `Dictionary`.
   - `UnitsApiController` duyệt danh sách -> Phân loại thống kê (Đếm số Nam/Nữ, tính tuổi từ ngày sinh, kiểm tra Dân tộc...).
   - Lưu kết quả thống kê (Summary) dưới dạng chuỗi JSON vào cột `SummaryJson` của bảng `Units`.
3. **Frontend**: Gọi API `GetSummary` -> Hiển thị số liệu lên giao diện báo cáo chuyên nghiệp.

---

## 🛠️ Các Điểm cần lưu ý khi nâng cấp hoặc sửa lỗi

1. **Database**: Khi thay đổi Entity (trong `Models`), cần chạy Migration hoặc cập nhật database tương ứng.
2. **Permissions**: Mọi API mới đều nên được gắn `[ApiAuthorize(Permission = "...")]` để đảm bảo bảo mật.
3. **Excel Template**: `ExcelService` được thiết kế để bóc tách dữ liệu theo mẫu chuẩn. Nếu mẫu Excel thay đổi (thêm/bớt cột), cần cập nhật mapping trong `UnitsApiController.BuildSummary`.

---
*Tài liệu được tạo bởi Antigravity AI để hỗ trợ quản lý dự án.*

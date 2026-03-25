# Hệ Thống Quản Lý Đoàn Viên (QuanLyDoanVien)

Dự án Hệ thống Quản lý Đoàn viên là một ứng dụng Web toàn diện ứng dụng công nghệ hiện đại, bao gồm Backend bằng ASP.NET Web API và Frontend bằng Angular. Hệ thống cung cấp khả năng quản lý chi tiết thông tin đoàn viên, đảng viên với các tính năng nhập/xuất dữ liệu từ Excel mạnh mẽ.

## Sơ đồ Kiến trúc Công nghệ

- **Backend:** ASP.NET Web API (.NET Framework 4.7.2)
- **Frontend:** Angular (v16+)
- **Database:** Microsoft SQL Server (sử dụng Entity Framework 6 - Code First / Database First)
- **Authentication:** JWT Bearer Token

## Cấu trúc thư mục (Directory Structure)

```
C:\QuanLyDoanVien\
│
├── QuanLyDoanVien.Web/       # Backend - Chứa mã nguồn ASP.NET Web API, Controllers, Services, Models.
├── QuanLyDoanVien.Angular/   # Frontend - Chứa source code giao diện trên nền tại Angular.
├── Database/              # Script SQL để khởi tạo Database và thiết lập Seed Data (Menu, Phân quyền).
└── QuanLyDoanVien.sln        # Visual Studio Solution File cho Backend.
```

## Các Module Chính

1.  **Quản trị Hệ thống (HE_THONG):** Quản lý người dùng, phân quyền vai trò (Role-based Access Control), quản lý danh mục chung, nhật ký thao tác (Audit logs).
2.  **Quản lý Đoàn viên (DOAN_VIEN):** Phân hệ chuyên biệt giúp quản lý chi tiết thông tin đoàn viên, phân loại đoàn viên/đảng viên, với khả năng Import dữ liệu từ file Excel có cấu trúc Header phức tạp, bao gồm các trường Ghi chú Cấp ủy, Giới tính, Chức vụ.

## Hướng dẫn Cài đặt & Khởi chạy (Local Development)

### 1. Cài đặt Cơ sở dữ liệu (Database)

Hệ thống sử dụng SQL Server LocalDB cho môi trường phát triển (`(localdb)\MSSQLLocalDB`).

1. Mở SQL Server Management Studio (SSMS) kết nối vào `(localdb)\MSSQLLocalDB`.
2. Mở thư mục `Database` và chạy lần lượt các script:
   - `001_CreateDatabase.sql`: Tạo Database `QuanLyDoanVien` và các bảng cơ sở.
   - `002_SeedData.sql`: Nạp dữ liệu cứng ban đầu (Tài khoản Admin, Vai trò, Menu).
3. Tài khoản quản trị mặc định:
   - **Username:** admin
   - **Password:** Admin@2024

### 2. Khởi chạy Backend (ASP.NET Web API)

1. Mở file `QuanLyDoanVien.sln` bằng **Visual Studio 2022**.
2. Kiểm tra chuỗi kết nối (Connection String) trong file `QuanLyDoanVien.Web\Web.config`:
   
   `<add name="QuanLyDoanVienContext" connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QuanLyDoanVien;Integrated Security=True;..." providerName="System.Data.SqlClient" />`
   
3. Nhấn `F5` hoặc chọn **IIS Express** (Port `52369`) để chạy dự án Backend. 
4. URL Backend gốc sẽ là: `http://localhost:52369/`

### 3. Khởi chạy Frontend (Angular)

Yêu cầu đã cài đặt **Node.js** và **Angular CLI** (`npm install -g @angular/cli`).

1. Mở Command Prompt (Terminal) tại thư mục Frontend:
   ```bash
   cd C:\QuanLyDoanVien\QuanLyDoanVien.Angular
   ```
2. Cài đặt các gói thư viện Node Modules:
   ```bash
   npm install
   ```
3. Khởi động Webpack Dev Server:
   ```bash
   npm start
   # Hoặc: ng serve --port 4200
   ```
4. Mở trình duyệt và truy cập `http://localhost:4200/`. Đăng nhập bằng tài khoản `admin`.

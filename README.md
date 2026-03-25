# Há»‡ Thá»‘ng Quáº£n LÃ½ ÄoÃ n ViÃªn (QuanLyDoanVien)

Dá»± Ã¡n Há»‡ thá»‘ng Quáº£n lÃ½ ÄoÃ n viÃªn lÃ  má»™t á»©ng dá»¥ng Web toÃ n diá»‡n á»©ng dá»¥ng cÃ´ng nghá»‡ hiá»‡n Ä‘áº¡i, bao gá»“m Backend báº±ng ASP.NET Web API vÃ  Frontend báº±ng Angular. Há»‡ thá»‘ng cung cáº¥p kháº£ nÄƒng quáº£n lÃ½ chi tiáº¿t thÃ´ng tin Ä‘oÃ n viÃªn, Ä‘áº£ng viÃªn vá»›i cÃ¡c tÃ­nh nÄƒng nháº­p/xuáº¥t dá»¯ liá»‡u tá»« Excel máº¡nh máº½.

## SÆ¡ Ä‘á»“ Kiáº¿n trÃºc CÃ´ng nghá»‡

- **Backend:** ASP.NET Web API (.NET Framework 4.7.2)
- **Frontend:** Angular (v16+)
- **Database:** Microsoft SQL Server (sá»­ dá»¥ng Entity Framework 6 - Code First / Database First)
- **Authentication:** JWT Bearer Token

## Cáº¥u trÃºc thÆ° má»¥c (Directory Structure)

```
C:\QuanLyDoanVien\
â”‚
â”œâ”€â”€ QuanLyDoanVien.Web/       # Backend - Chá»©a mÃ£ nguá»“n ASP.NET Web API, Controllers, Services, Models.
â”œâ”€â”€ QuanLyDoanVien.Angular/   # Frontend - Chá»©a source code giao diá»‡n trÃªn ná»n táº£ng Angular.
â”œâ”€â”€ Database/              # Script SQL Ä‘á»ƒ khá»Ÿi táº¡o Database vÃ  thiáº¿t láº­p Seed Data (Menu, PhÃ¢n quyá»n).
â””â”€â”€ QuanLyDoanVien.sln        # Visual Studio Solution File cho Backend.
```

## CÃ¡c Module ChÃ­nh

1.  **Quáº£n trá»‹ Há»‡ thá»‘ng (HE_THONG):** Quáº£n lÃ½ ngÆ°á»i dÃ¹ng, phÃ¢n quyá»n vai trÃ² (Role-based Access Control), quáº£n lÃ½ danh má»¥c chung, nháº­t kÃ½ thao tÃ¡c (Audit logs).
7.  **Quáº£n lÃ½ ÄoÃ n viÃªn (DOAN_VIEN):** PhÃ¢n há»‡ chuyÃªn biá»‡t giÃºp quáº£n lÃ½ chi tiáº¿t thÃ´ng tin Ä‘oÃ n viÃªn, phÃ¢n loáº¡i Ä‘oÃ n viÃªn/Ä‘áº£ng viÃªn, vá»›i kháº£ nÄƒng Import dá»¯ liá»‡u tá»« file Excel cÃ³ cáº¥u trÃºc Header phá»©c táº¡p, bao gá»“m cÃ¡c trÆ°á»ng Ghi chÃº Cáº¥p á»§y, Giá»›i tÃ­nh, Chá»©c vá»¥.

## HÆ°á»›ng dáº«n CÃ i Ä‘áº·t & Khá»Ÿi cháº¡y (Local Development)

### 1. CÃ i Ä‘áº·t CÆ¡ sá»Ÿ dá»¯ liá»‡u (Database)

Há»‡ thá»‘ng sá»­ dá»¥ng SQL Server LocalDB cho mÃ´i trÆ°á»ng phÃ¡t triá»ƒn (`(localdb)\MSSQLLocalDB`).

1. Má»Ÿ SQL Server Management Studio (SSMS) káº¿t ná»‘i vÃ o `(localdb)\MSSQLLocalDB`.
2. Má»Ÿ thÆ° má»¥c `Database` vÃ  cháº¡y láº§n lÆ°á»£t cÃ¡c script:
   - `001_CreateDatabase.sql`: Táº¡o Database `QuanLyDoanVien` vÃ  cÃ¡c báº£ng cÆ¡ sá»Ÿ.
   - `002_SeedData.sql`: Náº¡p dá»¯ liá»‡u cá»©ng ban Ä‘áº§u (TÃ i khoáº£n Admin, Vai trÃ², Menu).
3. TÃ i khoáº£n quáº£n trá»‹ máº·c Ä‘á»‹nh:
   - **Username:** admin
   - **Password:** Admin@2024

### 2. Khá»Ÿi cháº¡y Backend (ASP.NET Web API)

1. Má»Ÿ file `QuanLyDoanVien.sln` báº±ng **Visual Studio 2022**.
2. Kiá»ƒm tra chuá»—i káº¿t ná»‘i (Connection String) trong file `QuanLyDoanVien.Web\Web.config`:
   
   <add name="QuanLyDoanVienContext" connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QuanLyDoanVien;Integrated Security=True;..." providerName="System.Data.SqlClient" />
   
3. Nháº¥n `F5` hoáº·c chá»n **IIS Express** (Port `52369`) Ä‘á»ƒ cháº¡y dá»± Ã¡n Backend. 
4. URL Backend gá»‘c sáº½ lÃ : `http://localhost:52369/`

### 3. Khá»Ÿi cháº¡y Frontend (Angular)

YÃªu cáº§u Ä‘Ã£ cÃ i Ä‘áº·t **Node.js** vÃ  **Angular CLI** (`npm install -g @angular/cli`).

1. Má»Ÿ Command Prompt (Terminal) táº¡i thÆ° má»¥c Frontend:
   ```bash
   cd C:\QuanLyDoanVien\QuanLyDoanVien.Angular
   ```
2. CÃ i Ä‘áº·t cÃ¡c gÃ³i thÆ° viá»‡n Node Modules:
   ```bash
   npm install
   ```
3. Khá»Ÿi Ä‘á»™ng Webpack Dev Server:
   ```bash
   npm start
   # Hoáº·c: ng serve --port 4200
   ```
4. Má»Ÿ trÃ¬nh duyá»‡t vÃ  truy cáº­p `http://localhost:4200/`. ÄÄƒng nháº­p báº±ng tÃ i khoáº£n `admin`.




# =============================================================================
# deploy-iis.ps1
# Script triển khai QuanLyDoanVien lên IIS - Tuần 15
#
# CÁCH CHẠY:
#   1. Mở PowerShell với quyền Administrator
#   2. cd đến thư mục chứa script này
#   3. .\deploy-iis.ps1
#
# YÊU CẦU:
#   - Windows 10/11 hoặc Windows Server 2016+
#   - Visual Studio đã Build thành công ở chế độ Release
#   - SQL Server đang chạy (LocalDB hoặc SQL Server Express)
# =============================================================================

param(
    [string]$DeployPath   = "C:\Deploy\QuanLyDoanVien",
    [string]$PoolName     = "QuanLyDoanVienPool",
    [string]$SiteName     = "QuanLyDoanVien",
    [int]   $Port         = 8080,
    [string]$SourcePath   = "C:\QuanLyDoanVien\QuanLyDoanVien.Web"
)

# ─── Màu sắc cho output ──────────────────────────────────────────────────────
function Write-Step  { param($msg) Write-Host "`n▶ $msg" -ForegroundColor Cyan }
function Write-OK    { param($msg) Write-Host "  ✓ $msg" -ForegroundColor Green }
function Write-Warn  { param($msg) Write-Host "  ⚠ $msg" -ForegroundColor Yellow }
function Write-Fail  { param($msg) Write-Host "  ✗ $msg" -ForegroundColor Red }

# ─── Kiểm tra quyền Admin ────────────────────────────────────────────────────
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Write-Fail "Script cần quyền Administrator. Hãy chạy PowerShell với 'Run as Administrator'."
    exit 1
}

Write-Host "============================================================" -ForegroundColor Magenta
Write-Host "  DEPLOY QuanLyDoanVien lên IIS - Tuần 15" -ForegroundColor Magenta
Write-Host "  Deploy Path : $DeployPath" -ForegroundColor Magenta
Write-Host "  Pool Name   : $PoolName" -ForegroundColor Magenta
Write-Host "  Port        : $Port" -ForegroundColor Magenta
Write-Host "============================================================" -ForegroundColor Magenta

# ═══════════════════════════════════════════════════════════════════════════════
# BƯỚC 1: Cài đặt IIS và các tính năng cần thiết
# ═══════════════════════════════════════════════════════════════════════════════
Write-Step "Bước 1: Kiểm tra và cài đặt IIS..."

$iisFeatures = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-DefaultDocument",
    "IIS-HttpErrors",
    "IIS-StaticContent",
    "IIS-HttpRedirect",
    "IIS-ApplicationDevelopment",
    "IIS-ASPNET45",
    "IIS-NetFxExtensibility45",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter",
    "IIS-HttpCompressionStatic",
    "IIS-ManagementConsole"
)

$needInstall = $false
foreach ($feature in $iisFeatures) {
    $state = (Get-WindowsOptionalFeature -Online -FeatureName $feature -ErrorAction SilentlyContinue).State
    if ($state -ne "Enabled") {
        $needInstall = $true
        break
    }
}

if ($needInstall) {
    Write-Warn "Đang cài đặt IIS (có thể mất vài phút)..."
    Enable-WindowsOptionalFeature -Online -FeatureName $iisFeatures -All -NoRestart | Out-Null
    Write-OK "IIS đã được cài đặt thành công."
} else {
    Write-OK "IIS đã được cài đặt sẵn."
}

# Import WebAdministration module (quản lý IIS qua PowerShell)
Import-Module WebAdministration -ErrorAction Stop

# ═══════════════════════════════════════════════════════════════════════════════
# BƯỚC 2: Publish ứng dụng (copy file đã build sang thư mục deploy)
# ═══════════════════════════════════════════════════════════════════════════════
Write-Step "Bước 2: Publish ứng dụng sang $DeployPath ..."

# Kiểm tra thư mục bin/Release đã build chưa
$buildOutput = Join-Path $SourcePath "bin"
if (-not (Test-Path $buildOutput)) {
    Write-Fail "Chưa tìm thấy thư mục bin\. Hãy Build project trong Visual Studio trước (Ctrl+Shift+B)."
    exit 1
}

# Tạo thư mục deploy nếu chưa có
if (-not (Test-Path $DeployPath)) {
    New-Item -ItemType Directory -Path $DeployPath -Force | Out-Null
    Write-OK "Đã tạo thư mục: $DeployPath"
}

# Copy toàn bộ file ứng dụng
Write-Warn "Đang copy files..."
$excludeDirs = @("obj", ".git", "node_modules", "Uploads")
$items = Get-ChildItem -Path $SourcePath -Exclude $excludeDirs
foreach ($item in $items) {
    Copy-Item -Path $item.FullName -Destination $DeployPath -Recurse -Force
}

# Tạo thư mục Uploads nếu chưa có
$uploadsPath = Join-Path $DeployPath "Uploads"
if (-not (Test-Path $uploadsPath)) {
    New-Item -ItemType Directory -Path $uploadsPath -Force | Out-Null
}

Write-OK "Đã copy files thành công vào $DeployPath"

# ═══════════════════════════════════════════════════════════════════════════════
# BƯỚC 3: Tạo Application Pool
# ═══════════════════════════════════════════════════════════════════════════════
Write-Step "Bước 3: Tạo Application Pool '$PoolName' ..."

if (Test-Path "IIS:\AppPools\$PoolName") {
    Write-Warn "Application Pool '$PoolName' đã tồn tại → Đang cập nhật cấu hình..."
    Stop-WebAppPool -Name $PoolName -ErrorAction SilentlyContinue
} else {
    New-WebAppPool -Name $PoolName | Out-Null
    Write-OK "Đã tạo Application Pool: $PoolName"
}

# Cấu hình Pool
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "managedRuntimeVersion"  -Value "v4.0"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "managedPipelineMode"    -Value "Integrated"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "startMode"              -Value "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "autoStart"              -Value $true

# Pool Identity = ApplicationPoolIdentity (tài khoản ảo an toàn nhất)
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "processModel.identityType" -Value 4

# Tự động khởi động lại khi crash
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "failure.rapidFailProtection"      -Value $false
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "processModel.idleTimeout"         -Value "00:00:00"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name "recycling.periodicRestart.time"   -Value "00:00:00"

Write-OK "Đã cấu hình Application Pool: .NET 4.0 | Integrated | ApplicationPoolIdentity"

# ═══════════════════════════════════════════════════════════════════════════════
# BƯỚC 4: Cấp quyền thư mục cho Pool Identity
# ═══════════════════════════════════════════════════════════════════════════════
Write-Step "Bước 4: Cấp quyền thư mục cho 'IIS AppPool\$PoolName' ..."

$poolUser = "IIS AppPool\$PoolName"

# --- Quyền ReadAndExecute cho toàn bộ thư mục web ---
$acl = Get-Acl $DeployPath
$ruleRead = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $poolUser, "ReadAndExecute",
    "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.SetAccessRule($ruleRead)
Set-Acl $DeployPath $acl
Write-OK "Đã cấp ReadAndExecute vào: $DeployPath"

# --- Quyền Modify cho thư mục Uploads (upload file) ---
$aclUploads = Get-Acl $uploadsPath
$ruleModify = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $poolUser, "Modify",
    "ContainerInherit,ObjectInherit", "None", "Allow"
)
$aclUploads.SetAccessRule($ruleModify)
Set-Acl $uploadsPath $aclUploads
Write-OK "Đã cấp Modify vào: $uploadsPath"

# ═══════════════════════════════════════════════════════════════════════════════
# BƯỚC 5: Tạo Website trên IIS
# ═══════════════════════════════════════════════════════════════════════════════
Write-Step "Bước 5: Tạo Website IIS '$SiteName' trên cổng $Port ..."

# Kiểm tra port có bị chiếm không
$portInUse = Get-WebBinding | Where-Object { $_.bindingInformation -like "*:$Port:*" }
if ($portInUse -and -not (Test-Path "IIS:\Sites\$SiteName")) {
    Write-Fail "Cổng $Port đã được dùng bởi một site khác! Hãy đổi biến `$Port ở đầu script."
    exit 1
}

if (Test-Path "IIS:\Sites\$SiteName") {
    Write-Warn "Website '$SiteName' đã tồn tại → Đang cập nhật..."
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name "physicalPath"    -Value $DeployPath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name "applicationPool" -Value $PoolName
} else {
    New-Website -Name $SiteName `
        -Port $Port `
        -PhysicalPath $DeployPath `
        -ApplicationPool $PoolName | Out-Null
    Write-OK "Đã tạo website: $SiteName → http://localhost:$Port"
}

# Khởi động Pool và Site
Start-WebAppPool -Name $PoolName -ErrorAction SilentlyContinue
Start-Website    -Name $SiteName -ErrorAction SilentlyContinue
Write-OK "Application Pool và Website đã được khởi động."

# ═══════════════════════════════════════════════════════════════════════════════
# BƯỚC 6: Mã hóa Connection String (DPAPI)
# ═══════════════════════════════════════════════════════════════════════════════
Write-Step "Bước 6: Mã hóa connectionStrings trong Web.config ..."

$aspnetRegiis = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe"
if (-not (Test-Path $aspnetRegiis)) {
    $aspnetRegiis = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe"
}

if (Test-Path $aspnetRegiis) {
    # Kiểm tra xem đã mã hóa chưa (tránh mã hóa 2 lần)
    $webConfigContent = Get-Content (Join-Path $DeployPath "Web.config") -Raw
    if ($webConfigContent -match "configProtectionProvider") {
        Write-Warn "Connection String đã được mã hóa trước đó → Bỏ qua bước này."
    } else {
        Push-Location $DeployPath
        & $aspnetRegiis -pef "connectionStrings" . -prov "DataProtectionConfigurationProvider" | Out-Null
        Pop-Location

        if ($LASTEXITCODE -eq 0) {
            Write-OK "Đã mã hóa connectionStrings thành công bằng DPAPI."
        } else {
            Write-Warn "Mã hóa thất bại (exit code $LASTEXITCODE). Kiểm tra quyền Administrator."
        }
    }
} else {
    Write-Warn "Không tìm thấy aspnet_regiis.exe → Bỏ qua mã hóa."
    Write-Warn "Hãy chạy thủ công: $aspnetRegiis -pef connectionStrings `"$DeployPath`""
}

# ═══════════════════════════════════════════════════════════════════════════════
# TỔNG KẾT
# ═══════════════════════════════════════════════════════════════════════════════
Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  ✅ DEPLOY THÀNH CÔNG!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Backend API : http://localhost:$Port/api/auth/login" -ForegroundColor White
Write-Host "  Profiler    : http://localhost:$Port/api/profiler/results" -ForegroundColor White
Write-Host ""
Write-Host "  BƯỚC TIẾP THEO (thủ công):" -ForegroundColor Yellow
Write-Host "  1. Mở SSMS → chạy: scripts\grant-sql-permissions.sql" -ForegroundColor Yellow
Write-Host "  2. Cập nhật Connection String trong Web.config trỏ đến SQL Server thật" -ForegroundColor Yellow
Write-Host "  3. Build Angular: cd QuanLyDoanVien.Angular && ng build --configuration=production" -ForegroundColor Yellow
Write-Host "  4. Copy thư mục dist/ vào $DeployPath\dist" -ForegroundColor Yellow
Write-Host ""

# =============================================================================
# build-angular-prod.ps1
# Build Angular frontend cho môi trường production
#
# CÁCH CHẠY:
#   .\scripts\build-angular-prod.ps1
#   .\scripts\build-angular-prod.ps1 -ApiUrl "http://myserver.com"
# =============================================================================

param(
    [string]$ApiUrl     = "http://localhost:8080",
    [string]$DeployPath = "C:\Deploy\QuanLyDoanVien\dist"
)

$AngularDir = Join-Path $PSScriptRoot "..\QuanLyDoanVien.Angular"
$AngularDir = Resolve-Path $AngularDir

function Write-Step { param($msg) Write-Host "`n▶ $msg" -ForegroundColor Cyan }
function Write-OK   { param($msg) Write-Host "  ✓ $msg" -ForegroundColor Green }
function Write-Fail { param($msg) Write-Host "  ✗ $msg" -ForegroundColor Red }

Write-Host "============================================================" -ForegroundColor Magenta
Write-Host "  BUILD Angular Production" -ForegroundColor Magenta
Write-Host "  API URL     : $ApiUrl" -ForegroundColor Magenta
Write-Host "  Output      : $DeployPath" -ForegroundColor Magenta
Write-Host "============================================================" -ForegroundColor Magenta

# ─── Bước 1: Kiểm tra Node.js và Angular CLI ─────────────────────────────────
Write-Step "Bước 1: Kiểm tra môi trường..."

try {
    $nodeVersion = node --version 2>&1
    Write-OK "Node.js: $nodeVersion"
} catch {
    Write-Fail "Node.js chưa được cài đặt! Tải về tại https://nodejs.org"
    exit 1
}

try {
    $ngVersion = npx ng version 2>&1 | Select-String "Angular CLI"
    Write-OK "Angular CLI: $ngVersion"
} catch {
    Write-Fail "Angular CLI chưa được cài đặt! Chạy: npm install -g @angular/cli"
    exit 1
}

# ─── Bước 2: Cập nhật environment.prod.ts ────────────────────────────────────
Write-Step "Bước 2: Cập nhật API URL trong environment.prod.ts..."

$envProdFile = Join-Path $AngularDir "src\environments\environment.prod.ts"
$envContent = @"
export const environment = {
  production: true,
  apiBase: '$ApiUrl/api'
};
"@

Set-Content -Path $envProdFile -Value $envContent -Encoding UTF8
Write-OK "Đã cập nhật: apiBase = $ApiUrl/api"

# ─── Bước 3: Cài dependencies (nếu chưa có node_modules) ────────────────────
Write-Step "Bước 3: Kiểm tra dependencies..."

$nodeModules = Join-Path $AngularDir "node_modules"
if (-not (Test-Path $nodeModules)) {
    Write-Host "  Đang chạy npm install..." -ForegroundColor Yellow
    Push-Location $AngularDir
    npm install --silent
    Pop-Location
    Write-OK "Đã cài xong dependencies."
} else {
    Write-OK "node_modules đã tồn tại → bỏ qua npm install."
}

# ─── Bước 4: Build production ─────────────────────────────────────────────────
Write-Step "Bước 4: Build Angular production (có thể mất 1-2 phút)..."

Push-Location $AngularDir

$buildOutput = npx ng build --configuration=production 2>&1
$buildSuccess = $LASTEXITCODE -eq 0

Pop-Location

if ($buildSuccess) {
    Write-OK "Build thành công!"
} else {
    Write-Fail "Build thất bại! Kiểm tra lỗi bên dưới:"
    Write-Host $buildOutput -ForegroundColor Red
    exit 1
}

# ─── Bước 5: Copy dist sang thư mục deploy ────────────────────────────────────
Write-Step "Bước 5: Copy dist vào $DeployPath..."

$distSource = Join-Path $AngularDir "dist"

# Tìm thư mục con trong dist (Angular thường tạo dist/ten-project/)
$distSubDir = Get-ChildItem $distSource -Directory | Select-Object -First 1
if ($distSubDir) {
    $distSource = $distSubDir.FullName
}

if (-not (Test-Path $DeployPath)) {
    New-Item -ItemType Directory -Path $DeployPath -Force | Out-Null
}

Copy-Item -Path "$distSource\*" -Destination $DeployPath -Recurse -Force
Write-OK "Đã copy dist vào: $DeployPath"

# ─── Kết quả ─────────────────────────────────────────────────────────────────
Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  ✅ BUILD ANGULAR THÀNH CÔNG!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Files output : $DeployPath" -ForegroundColor White
Write-Host "  Để phục vụ static files từ IIS backend," -ForegroundColor Yellow
Write-Host "  mở HomeController.cs và thêm route trả về index.html" -ForegroundColor Yellow
Write-Host ""

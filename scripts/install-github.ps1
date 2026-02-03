#
# ClaudeCodeApiConfigManager (ccm) 一键安装脚本 (PowerShell - GitHub 源)
# 适用于 Windows
#
# 用法: irm https://raw.githubusercontent.com/morilong/ClaudeCodeApiConfigManager/master/scripts/install-github.ps1 | iex
#

$ErrorActionPreference = "Stop"

# 颜色输出函数
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

# 检测平台
function Get-PlatformInfo {
    $os = "win"
    $arch = if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") { "x64" }
            elseif ($env:PROCESSOR_ARCHITECTURE -eq "ARM64") { "arm64" }
            elseif ([Environment]::Is64BitProcess -and $env:PROCESSOR_ARCHITEW6432 -eq "AMD64") { "x64" }
            else { "x86" }

    return "${os}-${arch}"
}

# 获取最新版本
function Get-LatestVersion {
    Write-Info "获取最新版本信息..."

    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/repos/morilong/ClaudeCodeApiConfigManager/releases/latest" -ErrorAction Stop
        $version = $response.tag_name -replace '^v', ''
        if (-not $version) {
            throw "版本号为空"
        }
        Write-Success "最新版本: v$version"
        return $version
    }
    catch {
        Write-Error "无法从 GitHub API 获取版本信息"
        Write-Host ""
        Write-Host "请浏览器打开：" -ForegroundColor Yellow
        Write-Host "https://github.com/morilong/ClaudeCodeApiConfigManager/releases" -ForegroundColor Cyan
        Write-Host "手动下载最新版安装！" -ForegroundColor Yellow
        Write-Host ""
        exit 1
    }
}

# 下载并解压
function Download-AndExtract {
    param([string]$Version, [string]$Platform)

    # 创建临时目录
    $tmpDir = Join-Path $env:TEMP "ccm-install-$(Get-Random)"
    New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null
    Set-Location $tmpDir

    # 构建下载 URL (GitHub)
    if ($Platform -eq "win-x64" -or $Platform -eq "win-arm64" -or $Platform -eq "win-x86") {
        $filename = "ccm-${Platform}.zip"
    } else {
        Write-Error "不支持的平台: $Platform"
        exit 1
    }

    $downloadUrl = "https://github.com/morilong/ClaudeCodeApiConfigManager/releases/download/v${Version}/${filename}"

    Write-Info "正在下载: $downloadUrl"

    # 下载文件
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $filename -UseBasicParsing
        Write-Success "下载完成"
    }
    catch {
        Write-Error "下载失败，请检查网络连接或版本号"
        CleanUp $tmpDir
        exit 1
    }

    # 解压文件
    Write-Info "正在解压..."
    Expand-Archive -Path $filename -DestinationPath . -Force

    # 检查解压结果
    $exePath = Join-Path $tmpDir "ccm.exe"
    if (-not (Test-Path $exePath)) {
        Write-Error "解压后未找到 ccm.exe 可执行文件"
        CleanUp $tmpDir
        exit 1
    }

    Write-Success "解压完成"

    return $tmpDir
}

# 执行安装
function Run-Install {
    param([string]$TmpDir)

    Write-Info "正在执行安装..."

    $exePath = Join-Path $TmpDir "ccm.exe"

    # 执行 ccm install -y
    $process = Start-Process -FilePath $exePath -ArgumentList "install", "-y" -Wait -PassThru -NoNewWindow

    if ($process.ExitCode -eq 0) {
        # 安装完成
        Write-Host ""
    } else {
        Write-Error "安装失败 (退出代码: $($process.ExitCode))"
        CleanUp $TmpDir
        exit 1
    }

    # 清理临时文件
    CleanUp $TmpDir
}

# 清理临时文件
function CleanUp {
    param([string]$TmpDir)

    Set-Location $env:USERPROFILE
    Remove-Item -Path $TmpDir -Recurse -Force -ErrorAction SilentlyContinue
}

# 主流程
function Main {
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "  ClaudeCodeApiConfigManager 安装程序" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""

    $platform = Get-PlatformInfo
    Write-Info "检测到平台: $platform"

    $version = Get-LatestVersion
    $tmpDir = Download-AndExtract -Version $version -Platform $platform
    Run-Install -TmpDir $tmpDir

    Write-Success "ccm 已成功安装！"
    Write-Host ""
}

# 检查管理员权限（如果需要）
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if ($isAdmin) {
    Write-Warning "正在以管理员身份运行"
}

# 运行主流程
Main
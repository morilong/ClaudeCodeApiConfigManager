@echo off
REM 切换到 UTF-8 代码页以正确显示中文
chcp 65001 >nul
REM ClaudeCodeApiConfigManager (ccm) 一键安装脚本 (CMD - GitHub 源)
REM 适用于 Windows (CMD 批处理)
REM
REM 用法: curl -fsSL https://raw.githubusercontent.com/morilong/ClaudeCodeApiConfigManager/master/scripts/install-github.cmd -o install-github.cmd && install-github.cmd && del install-github.cmd
REM

setlocal enabledelayedexpansion

echo.
echo ==========================================
echo   ClaudeCodeApiConfigManager 安装程序
echo ==========================================
echo.

REM 检测架构
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set ARCH=x64
) else if "%PROCESSOR_ARCHITECTURE%"=="ARM64" (
    set ARCH=arm64
) else if defined PROCESSOR_ARCHITEW6432 (
    set ARCH=x64
) else (
    set ARCH=x86
)

set PLATFORM=win-%ARCH%
echo [INFO] 检测到平台: %PLATFORM%

REM 获取最新版本
echo [INFO] 获取最新版本信息...
for /f "usebackq tokens=*" %%i in (`powershell -Command "$r = try { Invoke-RestMethod -Uri 'https://api.github.com/repos/morilong/ClaudeCodeApiConfigManager/releases/latest' -ErrorAction Stop } catch { $null }; if ($r -and $r.tag_name) { $r.tag_name -replace '^v', '' } else { $null }"`) do set VERSION=%%i
if "%VERSION%"=="" (
    echo [ERROR] 无法从 GitHub API 获取版本信息
    echo.
    echo 请浏览器打开：
    echo https://github.com/morilong/ClaudeCodeApiConfigManager/releases
    echo 手动下载最新版安装！
    echo.
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    exit /b 1
)
echo [INFO] 最新版本: v%VERSION%

REM 创建临时目录
set TMP_DIR=%TEMP%\ccm-install-%RANDOM%
mkdir "%TMP_DIR%" 2>nul
cd /d "%TMP_DIR%"

REM 构建下载 URL (GitHub)
set FILENAME=ccm-%PLATFORM%.zip
set DOWNLOAD_URL=https://github.com/morilong/ClaudeCodeApiConfigManager/releases/download/v%VERSION%/%FILENAME%

echo [INFO] 正在下载: %DOWNLOAD_URL%

REM 下载文件（使用 PowerShell）
powershell -Command "Invoke-WebRequest -Uri '%DOWNLOAD_URL%' -OutFile '%FILENAME%' -UseBasicParsing"

if not exist "%FILENAME%" (
    echo [ERROR] 下载失败，请检查网络连接或版本号
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    exit /b 1
)

echo [SUCCESS] 下载完成

REM 解压文件
echo [INFO] 正在解压...
powershell -Command "Expand-Archive -Path '%FILENAME%' -DestinationPath '.' -Force"

if not exist "ccm.exe" (
    echo [ERROR] 解压后未找到 ccm.exe 可执行文件
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    exit /b 1
)

echo [SUCCESS] 解压完成

REM 执行安装
echo [INFO] 正在执行安装...
ccm.exe install -y

if %ERRORLEVEL% EQU 0 (
    REM 安装完成
    echo.
) else (
    echo [ERROR] 安装失败
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    exit /b 1
)

REM 清理临时文件
cd /d "%TEMP%"
rmdir /s /q "%TMP_DIR%" 2>nul

echo [SUCCESS] ccm 已成功安装！
echo.

endlocal
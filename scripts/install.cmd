@echo off
setlocal enabledelayedexpansion

REM Switch to UTF-8 code page
chcp 65001 >nul 2>&1

echo.
echo ==========================================
echo   ClaudeCodeApiConfigManager Installer
echo ==========================================
echo.

REM Detect architecture
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
echo [INFO] Detected platform: %PLATFORM%

REM Get latest version
echo [INFO] Fetching latest version...

set VERSION=
for /f "delims=" %%i in ('powershell -Command "$ProgressPreference = 'SilentlyContinue'; try { (Invoke-RestMethod -Uri 'https://gitee.com/api/v5/repos/morilong/claude-code-api-config-manager/releases?page=1^&per_page=1^&direction=desc')[0].tag_name -replace '^v', '' } catch { '' }"') do set VERSION=%%i

if "%VERSION%"=="" (
    echo [ERROR] Failed to get version from Gitee API
    echo.
    echo Please visit:
    echo https://gitee.com/morilong/claude-code-api-config-manager/releases
    echo.
    endlocal
    exit /b 1
)
echo [INFO] Latest version: v%VERSION%

REM Create temp directory
set "TMP_DIR=%TEMP%\ccm-install-%RANDOM%"
mkdir "%TMP_DIR%" 2>nul
cd /d "%TMP_DIR%"

REM Build download URL
set "FILENAME=ccm-%PLATFORM%.zip"
set "DOWNLOAD_URL=https://gitee.com/morilong/claude-code-api-config-manager/releases/download/v%VERSION%/%FILENAME%"

echo [INFO] Downloading: %DOWNLOAD_URL%

REM Download file using PowerShell
powershell -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest -Uri '!DOWNLOAD_URL!' -OutFile '!FILENAME!' -UseBasicParsing"

if not exist "%FILENAME%" (
    echo [ERROR] Download failed
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    endlocal
    exit /b 1
)

echo [SUCCESS] Download complete

REM Extract file
echo [INFO] Extracting...
powershell -Command "Expand-Archive -Path '!FILENAME!' -DestinationPath '.' -Force"

if not exist "ccm.exe" (
    echo [ERROR] ccm.exe not found after extraction
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    endlocal
    exit /b 1
)

echo [SUCCESS] Extraction complete

REM Run installation
echo [INFO] Running installation...
ccm.exe install -y

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Installation failed
    cd /d "%TEMP%"
    rmdir /s /q "%TMP_DIR%" 2>nul
    endlocal
    exit /b 1
)

REM Cleanup
cd /d "%TEMP%"
rmdir /s /q "%TMP_DIR%" 2>nul

echo [SUCCESS] ccm installed successfully.
echo.

endlocal

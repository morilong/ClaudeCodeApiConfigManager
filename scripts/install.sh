#!/bin/bash
#
# ClaudeCodeApiConfigManager (ccm) 一键安装脚本 (Gitee 源)
# 适用于 Linux 和 macOS
#
# 用法: curl -fsSL https://gitee.com/morilong/claude-code-api-config-manager/raw/master/scripts/install.sh | bash
#

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 打印带颜色的消息
print_info() {
    printf "${BLUE}[INFO]${NC} %s\n" "$1"
}

print_success() {
    printf "${GREEN}[SUCCESS]${NC} %s\n" "$1"
}

print_error() {
    printf "${RED}[ERROR]${NC} %s\n" "$1"
}

print_warning() {
    printf "${YELLOW}[WARNING]${NC} %s\n" "$1"
}

# 检测操作系统和架构
detect_platform() {
    OS_TYPE=$(uname -s)
    ARCH_TYPE=$(uname -m)

    case "$OS_TYPE" in
        Linux*)
            OS="linux"
            ;;
        Darwin*)
            OS="osx"
            ;;
        *)
            print_error "不支持的操作系统: $OS_TYPE"
            exit 1
            ;;
    esac

    case "$ARCH_TYPE" in
        x86_64|amd64)
            ARCH="x64"
            ;;
        aarch64|arm64)
            ARCH="arm64"
            ;;
        i386|i686)
            print_error "不支持 32 位 x86 架构"
            exit 1
            ;;
        *)
            print_error "不支持的架构: $ARCH_TYPE"
            exit 1
            ;;
    esac

    PLATFORM="${OS}-${ARCH}"
    print_info "检测到平台: $PLATFORM"
}

# 获取最新版本号
get_latest_version() {
    print_info "获取最新版本信息..."

    # 从 Gitee API 获取最新版本
    VERSION=$(curl -fsSL "https://gitee.com/api/v5/repos/morilong/claude-code-api-config-manager/releases?page=1&per_page=1&direction=desc" | grep -o '"tag_name":"[^"]*"' | cut -d'"' -f4 | sed 's/^v//')

    if [ -z "$VERSION" ]; then
        print_error "无法从 Gitee API 获取版本信息"
        echo ""
        echo "请浏览器打开："
        echo "https://gitee.com/morilong/claude-code-api-config-manager/releases"
        echo "手动下载最新版安装！"
        echo ""
        exit 1
    else
        print_success "最新版本: v$VERSION"
    fi
}

# 下载并解压可执行文件
download_and_extract() {
    # 创建临时目录
    TMP_DIR=$(mktemp -d)
    cd "$TMP_DIR"

    # 构建下载 URL (Gitee)
    FILENAME="ccm-${PLATFORM}.tar.gz"
    DOWNLOAD_URL="https://gitee.com/morilong/claude-code-api-config-manager/releases/download/v${VERSION}/${FILENAME}"

    print_info "正在下载: $DOWNLOAD_URL"

    # 下载文件
    if ! curl -fSL -o "$FILENAME" "$DOWNLOAD_URL"; then
        print_error "下载失败，请检查网络连接或版本号"
        rm -rf "$TMP_DIR"
        exit 1
    fi

    print_success "下载完成"

    # 解压文件
    print_info "正在解压..."
    tar -xzf "$FILENAME"

    # 检查解压结果
    if [ ! -f "ccm" ]; then
        print_error "解压后未找到 ccm 可执行文件"
        rm -rf "$TMP_DIR"
        exit 1
    fi

    # 添加执行权限
    chmod +x ccm

    print_success "解压完成"
}

# 执行安装
run_install() {
    print_info "正在执行安装..."

    # 执行 ccm install -y
    if "./ccm" install -y; then
        # 安装完成
        echo ""
    else
        print_error "安装失败"
        cd - > /dev/null
        rm -rf "$TMP_DIR"
        exit 1
    fi

    # 清理临时文件
    cd - > /dev/null
    rm -rf "$TMP_DIR"
}

# 主流程
main() {
    echo ""
    echo "=========================================="
    echo "  ClaudeCodeApiConfigManager 安装程序"
    echo "=========================================="
    echo ""

    detect_platform
    get_latest_version
    download_and_extract
    run_install

    print_success "ccm 已成功安装。"
    echo ""
}

# 运行主流程
main

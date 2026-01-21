#!/bin/bash

# YOP SDK NuGet 打包和发布脚本
# 使用方法:
#   ./pack-and-publish.sh pack          # 仅打包
#   ./pack-and-publish.sh publish        # 打包并发布到 NuGet.org
#   ./pack-and-publish.sh test           # 打包并在本地测试

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SDK_DIR="$SCRIPT_DIR"
NUPKG_DIR="$SDK_DIR/nupkg"
PACKAGE_NAME="YOP.SDK"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查 .NET SDK
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK 未安装，请先安装 .NET SDK 10.0 或更高版本"
        exit 1
    fi
    
    DOTNET_VERSION=$(dotnet --version)
    print_info "检测到 .NET SDK 版本: $DOTNET_VERSION"
}

# 清理之前的构建
clean() {
    print_info "清理之前的构建..."
    cd "$SDK_DIR"
    dotnet clean -c Release > /dev/null 2>&1 || true
    rm -rf "$NUPKG_DIR"
}

# 构建项目
build() {
    print_info "构建项目..."
    cd "$SDK_DIR"
    dotnet build -c Release
}

# 打包
pack() {
    print_info "打包 NuGet 包..."
    cd "$SDK_DIR"
    mkdir -p "$NUPKG_DIR"
    
    dotnet pack -c Release -o "$NUPKG_DIR" --include-symbols --include-source
    
    if [ $? -eq 0 ]; then
        print_info "打包成功！"
        echo ""
        echo "生成的包文件:"
        ls -lh "$NUPKG_DIR"/*.nupkg "$NUPKG_DIR"/*.snupkg 2>/dev/null | awk '{print "  " $9 " (" $5 ")"}'
    else
        print_error "打包失败"
        exit 1
    fi
}

# 验证包
verify_package() {
    print_info "验证包内容..."
    cd "$SDK_DIR"
    
    NUPKG_FILE=$(ls "$NUPKG_DIR"/*.nupkg 2>/dev/null | head -n 1)
    
    if [ -z "$NUPKG_FILE" ]; then
        print_error "未找到 .nupkg 文件"
        exit 1
    fi
    
    print_info "包文件: $NUPKG_FILE"
    
    # 检查包大小
    PACKAGE_SIZE=$(stat -f%z "$NUPKG_FILE" 2>/dev/null || stat -c%s "$NUPKG_FILE" 2>/dev/null)
    PACKAGE_SIZE_MB=$((PACKAGE_SIZE / 1024 / 1024))
    
    if [ $PACKAGE_SIZE_MB -gt 250 ]; then
        print_warn "包大小超过 250MB，可能无法上传到 NuGet.org"
    else
        print_info "包大小: ${PACKAGE_SIZE_MB}MB (限制: 250MB)"
    fi
}

# 本地测试
test_package() {
    print_info "在本地测试包..."
    
    TEST_DIR="/tmp/yop-sdk-test-$$"
    mkdir -p "$TEST_DIR"
    cd "$TEST_DIR"
    
    # 创建测试项目
    print_info "创建测试项目..."
    dotnet new console -n TestPackage -f net10.0 > /dev/null 2>&1
    cd TestPackage
    
    # 获取包版本
    PACKAGE_VERSION=$(basename "$(ls "$NUPKG_DIR"/*.nupkg | head -n 1)" .nupkg | grep -o '\([0-9]\+\.[0-9]\+\.[0-9]\+\)')
    
    # 安装包 - 直接使用本地路径
    print_info "安装包 YOP.SDK 版本 $PACKAGE_VERSION..."
    if dotnet add package YOP.SDK --version "$PACKAGE_VERSION" --source "$NUPKG_DIR" > /dev/null 2>&1; then
        print_info "✓ 包安装成功"
        
        # 尝试构建
        print_info "构建测试项目..."
        if dotnet build > /dev/null 2>&1; then
            print_info "✓ 构建成功"
        else
            print_warn "构建失败，请检查依赖项"
        fi
    else
        print_error "包安装失败"
        exit 1
    fi
    
    # 清理
    cd /
    rm -rf "$TEST_DIR"
    print_info "测试完成"
}

# 发布到 NuGet.org
publish_to_nuget() {
    print_info "准备发布到 NuGet.org..."
    
    # 检查 API 密钥
    if [ -z "$NUGET_API_KEY" ]; then
        print_error "未设置 NUGET_API_KEY 环境变量"
        echo ""
        echo "请设置 API 密钥:"
        echo "  export NUGET_API_KEY='your-api-key'"
        echo ""
        echo "或者直接在命令中指定:"
        echo "  NUGET_API_KEY='your-api-key' ./pack-and-publish.sh publish"
        exit 1
    fi
    
    NUPKG_FILE=$(ls "$NUPKG_DIR"/*.nupkg 2>/dev/null | head -n 1)
    SNUPKG_FILE=$(ls "$NUPKG_DIR"/*.snupkg 2>/dev/null | head -n 1)
    
    if [ -z "$NUPKG_FILE" ]; then
        print_error "未找到 .nupkg 文件，请先执行打包"
        exit 1
    fi
    
    # 发布主包
    print_info "发布主包到 NuGet.org..."
    if dotnet nuget push "$NUPKG_FILE" \
        --api-key "$NUGET_API_KEY" \
        --source https://api.nuget.org/v3/index.json \
        --skip-duplicate; then
        print_info "✓ 主包发布成功"
    else
        print_error "主包发布失败"
        exit 1
    fi
    
    # 发布符号包
    if [ -n "$SNUPKG_FILE" ]; then
        print_info "发布符号包到 NuGet.org..."
        if dotnet nuget push "$SNUPKG_FILE" \
            --api-key "$NUGET_API_KEY" \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate; then
            print_info "✓ 符号包发布成功"
        else
            print_warn "符号包发布失败（可选）"
        fi
    fi
    
    print_info "发布完成！"
    print_info "包将在几分钟后出现在 NuGet.org 上"
    print_info "查看地址: https://www.nuget.org/packages/$PACKAGE_NAME"
}

# 主函数
main() {
    ACTION=${1:-pack}
    
    case "$ACTION" in
        pack)
            check_dotnet
            clean
            build
            pack
            verify_package
            ;;
        publish)
            check_dotnet
            clean
            build
            pack
            verify_package
            publish_to_nuget
            ;;
        test)
            check_dotnet
            if [ ! -d "$NUPKG_DIR" ] || [ -z "$(ls -A "$NUPKG_DIR"/*.nupkg 2>/dev/null)" ]; then
                print_warn "未找到包文件，先执行打包..."
                clean
                build
                pack
            fi
            verify_package
            test_package
            ;;
        *)
            echo "使用方法: $0 {pack|publish|test}"
            echo ""
            echo "命令说明:"
            echo "  pack     - 仅打包 NuGet 包"
            echo "  publish  - 打包并发布到 NuGet.org（需要设置 NUGET_API_KEY）"
            echo "  test     - 打包并在本地测试安装"
            exit 1
            ;;
    esac
}

main "$@"

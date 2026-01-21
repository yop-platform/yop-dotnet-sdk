# YOP .NET SDK NuGet 发布指南

本指南将帮助您将 YOP .NET SDK 打包并发布到 NuGet 仓库。

## 目录

- [准备工作](#准备工作)
- [本地打包](#本地打包)
- [验证包内容](#验证包内容)
- [发布到 NuGet.org](#发布到-nugetorg)
- [发布到私有 NuGet 源](#发布到私有-nuget-源)
- [版本管理](#版本管理)
- [常见问题](#常见问题)

---

## 准备工作

### 1. 检查项目配置

确保 `SDK/YOP.SDK.csproj` 文件包含完整的 NuGet 元数据：

```xml
<PropertyGroup>
  <PackageId>YOP.SDK</PackageId>
  <PackageVersion>4.0.0</PackageVersion>
  <Authors>Yeepay</Authors>
  <PackageDescription>Yeepay Open Platform .NET SDK for payment integration</PackageDescription>
  <PackageTags>yeepay;payment;sdk;yop;api</PackageTags>
  <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/yop-platform/yop-dotnet-sdk</PackageProjectUrl>
  <RepositoryUrl>https://github.com/yop-platform/yop-dotnet-sdk</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
</PropertyGroup>
```

### 2. 准备 API 密钥

#### 发布到 NuGet.org

1. 访问 [NuGet.org](https://www.nuget.org/)
2. 登录或注册账户
3. 进入 **Account Settings** → **API Keys**
4. 创建新的 API 密钥，设置：
   - **Key name**: 例如 "YOP.SDK Publishing Key"
   - **Expiration**: 选择合适的过期时间
   - **Glob pattern**: `YOP.SDK` 或 `YOP.SDK/*`（限制只能发布特定包）
5. 复制生成的 API 密钥（**只显示一次，请妥善保存**）

#### 发布到私有源

根据您的私有 NuGet 源提供商（如 Azure Artifacts、GitHub Packages、Nexus 等）获取相应的 API 密钥或访问令牌。

### 3. 安装必要工具

确保已安装：
- .NET SDK 10.0 或更高版本
- NuGet CLI（可选，用于高级操作）

```bash
# 检查 .NET SDK 版本
dotnet --version

# 安装 NuGet CLI（可选）
# Windows: 从 https://www.nuget.org/downloads 下载
# macOS/Linux: 使用 Mono 或通过 .NET CLI
```

---

## 本地打包

### 步骤 1: 清理之前的构建

```bash
cd SDK
dotnet clean -c Release
```

### 步骤 2: 构建项目

```bash
dotnet build -c Release
```

### 步骤 3: 打包

```bash
# 基本打包命令
dotnet pack -c Release

# 打包并指定输出目录
dotnet pack -c Release -o ./nupkg

# 打包并包含符号包（.snupkg）
dotnet pack -c Release --include-symbols --include-source -o ./nupkg
```

打包完成后，您会在输出目录（默认是 `bin/Release/`）找到：
- `YOP.SDK.4.0.0.nupkg` - NuGet 包文件
- `YOP.SDK.4.0.0.snupkg` - 符号包文件（如果包含符号）

### 步骤 4: 验证包文件

```bash
# 查看包内容（需要安装 NuGet CLI）
nuget list -Source ./nupkg

# 或使用 .NET CLI
dotnet nuget locals all --list
```

---

## 验证包内容

在发布之前，建议验证包的内容是否正确：

### 1. 检查包元数据

```bash
# 使用 NuGet Package Explorer（GUI 工具）
# 下载地址: https://github.com/NuGetPackageExplorer/NuGetPackageExplorer

# 或使用命令行工具
dotnet nuget verify ./nupkg/YOP.SDK.4.0.0.nupkg
```

### 2. 本地测试安装

创建一个测试项目来验证包是否可以正常安装和使用：

```bash
# 创建测试项目
cd ../.local/demos
dotnet new console -n TestNuGetPackage -f net10.0
cd TestNuGetPackage

# 添加本地包源
dotnet nuget add source ../../../SDK/nupkg --name local-yop-sdk

# 安装包
dotnet add package YOP.SDK --version 4.0.0 --source local-yop-sdk

# 验证安装
dotnet restore
dotnet build
```

### 3. 检查依赖项

确保包的所有依赖项都正确声明：

```bash
# 查看包依赖
dotnet list package --include-transitive
```

---

## 发布到 NuGet.org

### 方法一：使用 dotnet CLI（推荐）

#### 步骤 1: 设置 API 密钥

```bash
# 设置 NuGet.org 的 API 密钥
dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org

# 设置 API 密钥（将 YOUR_API_KEY 替换为实际的 API 密钥）
dotnet nuget update source nuget.org --username YOUR_USERNAME --password YOUR_API_KEY --store-password-in-clear-text
```

或者使用环境变量：

```bash
# macOS/Linux
export NUGET_API_KEY="YOUR_API_KEY"

# Windows PowerShell
$env:NUGET_API_KEY="YOUR_API_KEY"
```

#### 步骤 2: 发布包

```bash
cd SDK

# 发布主包
dotnet nuget push ./bin/Release/YOP.SDK.4.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# 如果使用环境变量
dotnet nuget push ./bin/Release/YOP.SDK.4.0.0.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json

# 同时发布符号包
dotnet nuget push ./bin/Release/YOP.SDK.4.0.0.snupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json --symbol-source https://api.nuget.org/v3/index.json
```

#### 步骤 3: 验证发布

1. 访问 [NuGet.org](https://www.nuget.org/packages/YOP.SDK)
2. 等待几分钟让包索引完成
3. 尝试安装包验证：

```bash
dotnet add package YOP.SDK --version 4.0.0
```

### 方法二：使用 NuGet CLI

```bash
# 设置 API 密钥
nuget setApiKey YOUR_API_KEY -Source https://api.nuget.org/v3/index.json

# 发布包
nuget push ./bin/Release/YOP.SDK.4.0.0.nupkg -Source https://api.nuget.org/v3/index.json

# 发布符号包
nuget push ./bin/Release/YOP.SDK.4.0.0.snupkg -Source https://api.nuget.org/v3/index.json
```

### 方法三：通过 NuGet.org 网站

1. 访问 [NuGet.org](https://www.nuget.org/packages/manage/upload)
2. 登录您的账户
3. 上传 `.nupkg` 文件
4. 填写包信息（如果元数据不完整）
5. 点击 **Submit**

---

## 发布到私有 NuGet 源

### Azure Artifacts

```bash
# 添加 Azure Artifacts 源
dotnet nuget add source https://pkgs.dev.azure.com/ORG/PROJECT/_packaging/FEED/nuget/v3/index.json --name azure-artifacts --username USERNAME --password PAT_TOKEN

# 发布包
dotnet nuget push ./bin/Release/YOP.SDK.4.0.0.nupkg --source azure-artifacts --api-key az
```

### GitHub Packages

```bash
# 添加 GitHub Packages 源
dotnet nuget add source https://nuget.pkg.github.com/OWNER/index.json --name github --username USERNAME --password GITHUB_TOKEN

# 发布包
dotnet nuget push ./bin/Release/YOP.SDK.4.0.0.nupkg --source github --api-key GITHUB_TOKEN
```

### 自托管 NuGet 服务器

```bash
# 添加私有源
dotnet nuget add source https://your-nuget-server.com/v3/index.json --name private-server --username USERNAME --password PASSWORD

# 发布包
dotnet nuget push ./bin/Release/YOP.SDK.4.0.0.nupkg --source private-server
```

---

## 版本管理

### 语义化版本控制

NuGet 使用[语义化版本控制](https://semver.org/)（SemVer）：
- **主版本号**（Major）：不兼容的 API 更改
- **次版本号**（Minor）：向后兼容的功能添加
- **修订号**（Patch）：向后兼容的 bug 修复
- **预发布标签**（可选）：`-alpha`, `-beta`, `-rc` 等

示例：
- `4.0.0` - 正式版本
- `4.1.0-alpha.1` - 预发布版本
- `4.0.1` - 补丁版本

### 更新版本号

#### 方法一：修改 .csproj 文件

```xml
<PropertyGroup>
  <PackageVersion>4.1.0</PackageVersion>
  <Version>4.1.0</Version>
</PropertyGroup>
```

#### 方法二：使用命令行参数

```bash
dotnet pack -c Release -p:PackageVersion=4.1.0 -p:Version=4.1.0
```

#### 方法三：使用 MSBuild 属性文件

创建 `Directory.Build.props`：

```xml
<Project>
  <PropertyGroup>
    <Version>4.1.0</Version>
    <PackageVersion>4.1.0</PackageVersion>
  </PropertyGroup>
</Project>
```

### 预发布版本

```bash
# 打包预发布版本
dotnet pack -c Release -p:PackageVersion=4.1.0-alpha.1

# 发布预发布版本（NuGet.org 默认接受预发布版本）
dotnet nuget push ./bin/Release/YOP.SDK.4.1.0-alpha.1.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

---

## 自动化发布

### 使用 GitHub Actions

创建 `.github/workflows/publish-nuget.yml`：

```yaml
name: Publish to NuGet

on:
  release:
    types: [created]
  workflow_dispatch:
    inputs:
      version:
        description: 'Package version'
        required: true

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
          
      - name: Build
        run: dotnet build SDK/YOP.SDK.csproj -c Release
        
      - name: Pack
        run: dotnet pack SDK/YOP.SDK.csproj -c Release --no-build -o ./nupkg
        
      - name: Push to NuGet
        run: dotnet nuget push ./nupkg/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

### 使用 Azure DevOps Pipeline

```yaml
trigger:
  - main

pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '10.0.x'
    
- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    projects: 'SDK/YOP.SDK.csproj'
    arguments: '-c Release'
    
- task: DotNetCoreCLI@2
  displayName: 'Pack'
  inputs:
    command: 'pack'
    packagesToPack: 'SDK/YOP.SDK.csproj'
    arguments: '-c Release --no-build -o $(Build.ArtifactStagingDirectory)'
    
- task: NuGetCommand@2
  displayName: 'Push to NuGet'
  inputs:
    command: 'push'
    packagesToPush: '$(Build.ArtifactStagingDirectory)/*.nupkg'
    nuGetFeedType: 'external'
    publishFeedCredentials: 'NuGet.org'
```

---

## 常见问题

### Q: 发布时提示 "包已存在"

**A:** NuGet.org 不允许覆盖已发布的包。您需要：
1. 增加版本号
2. 如果是预发布版本，可以删除后重新发布（需要 NuGet.org 账户权限）

### Q: 如何删除已发布的包？

**A:** NuGet.org 不允许删除已发布的包，但可以：
1. 取消列出（Unlist）：包仍然可以安装，但不会在搜索结果中显示
2. 通过 NuGet.org 网站 → 包管理页面 → 取消列出

### Q: 符号包（.snupkg）是什么？

**A:** 符号包包含调试信息，帮助开发者在调试时查看源代码。发布符号包是可选的，但建议发布以改善开发体验。

### Q: 如何发布到多个源？

**A:** 分别执行多个 `dotnet nuget push` 命令，或使用脚本：

```bash
#!/bin/bash
PACKAGE_PATH="./bin/Release/YOP.SDK.4.0.0.nupkg"
API_KEY="YOUR_API_KEY"

# 发布到 NuGet.org
dotnet nuget push $PACKAGE_PATH --api-key $API_KEY --source https://api.nuget.org/v3/index.json

# 发布到私有源
dotnet nuget push $PACKAGE_PATH --source https://your-private-source.com/v3/index.json
```

### Q: 包大小限制是多少？

**A:** 
- NuGet.org: 最大 250 MB
- 建议保持包大小在 10-50 MB 以内以获得最佳性能

### Q: 如何包含 README 文件到包中？

**A:** 在 `.csproj` 中添加：

```xml
<PropertyGroup>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>

<ItemGroup>
  <None Include="..\README.md" Pack="true" PackagePath="\"/>
</ItemGroup>
```

### Q: 如何设置包的最低 .NET 版本要求？

**A:** 通过 `TargetFramework` 属性设置：

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

---

## 最佳实践

1. **版本管理**：
   - 遵循语义化版本控制
   - 在 CHANGELOG.md 中记录所有变更
   - 使用 Git 标签标记发布版本

2. **测试**：
   - 发布前在本地测试安装和使用
   - 运行所有单元测试
   - 验证依赖项正确解析

3. **文档**：
   - 确保 README.md 完整且准确
   - 包含使用示例
   - 提供 API 文档链接

4. **安全性**：
   - 不要在代码中硬编码密钥
   - 使用环境变量或密钥管理服务存储 API 密钥
   - 定期轮换 API 密钥

5. **CI/CD**：
   - 自动化打包和发布流程
   - 使用 GitHub Actions 或 Azure DevOps
   - 在发布前运行自动化测试

---

## 相关资源

- [NuGet 官方文档](https://docs.microsoft.com/nuget/)
- [.NET CLI 文档](https://docs.microsoft.com/dotnet/core/tools/)
- [语义化版本控制](https://semver.org/)
- [NuGet.org 发布指南](https://docs.microsoft.com/nuget/nuget-org/publish-a-package)

---

**注意**: 发布到 NuGet.org 后，包可能需要几分钟时间才能被索引和搜索到。请耐心等待。

# YOP .NET SDK 升级到最新版本

## 当前状态分析

### 项目现状
- **当前框架**: .NET Framework 4.8
- **项目格式**: 旧版MSBuild格式 (ToolsVersion="14.0")
- **依赖管理**: packages.config + 直接DLL引用
- **主要依赖**: BouncyCastle.Crypto 1.8.0-beta4 (测试版本)

### 环境状况
- **系统.NET SDK**: 10.0.102 (.NET 10.0)
- **支持平台**: 跨平台支持

## 本次已完成变更摘要

### 依赖与项目配置
- **BouncyCastle 升级**：将历史 `BouncyCastle.Crypto 1.8.0-beta4` 替换为 `BouncyCastle.Cryptography 2.6.2`（兼容现代 .NET、持续维护）。
- **移除不必要依赖**：删除 `System.Text.Encodings.Web`、`System.Net.Http.Json` 等在现代 .NET 下不需要显式引用的包引用，消除相关 NU1510 告警。

### 网络请求（彻底移除 WebRequest）
- **移除**：`WebRequest/HttpWebRequest/HttpWebResponse/ServicePointManager`
- **替代**：统一改为 `HttpClient`（`SDK/yop.utils/HttpUtils.cs`）
- **方法更名**：
  - `HttpUtils.PostAndGetHttpWebResponse(...)` → `HttpUtils.Send(...)`
  - `HttpUtils.PostFile(...)` → `HttpUtils.SendMultipart(...)`
- **调用方同步**：`YopClient` / `YopRsaClient` 已改为读取 `HttpResponseMessage.Content`，RSA 场景从 response header 读取 `x-yop-sign` 做验签。

### 加密/签名（跨平台替换）
- **对称加密**：`Rijndael*` → `Aes`（消除 SYSLIB0022）
- **摘要算法**：`MD5CryptoServiceProvider/SHA256Managed` → `MD5.Create()/SHA256.Create()`（消除 SYSLIB0021）
- **时间戳**：`TimeZone.CurrentTimeZone` → `DateTimeOffset.ToUnixTimeMilliseconds()`（消除 CS0618）
- **RSA 跨平台统一**：
  - 清理 Windows-only P/Invoke：删除 `SDK/yop.utils/RSACryptoServiceProviderExtension.cs`（`advapi32.dll/crypt32.dll`）
  - 将 `RSACryptoService`、`RSAFromPkcs8`、`RsaAndAes` 等实现迁移到 `RSA.Create()` + `ImportParameters(...)` + `RSAEncryptionPadding/RSASignaturePadding`

### 验证方式（当前已通过）
```bash
# 使用.NET 8.0构建和测试
dotnet build
dotnet build -warnaserror -p:EnableNETAnalyzers=true -p:AnalysisLevel=latest
dotnet test --filter "FullyQualifiedName~YopQaFullTest"

# 验证.NET 10.0兼容性
dotnet add package YOP.SDK --version 4.0.0 --source /path/to/nupkg
dotnet build
```

## 升级目标

### 主要目标 ✅ 已完成
1. ✅ **升级到现代.NET**: 已迁移到.NET 8.0 LTS (稳定版本)
2. ✅ **项目格式现代化**: 已采用SDK风格项目文件
3. ✅ **依赖管理优化**: 已使用PackageReference替代packages.config
4. ✅ **代码规范统一**: 已统一编码标准和文档规范

### 次要目标 ✅ 已完成
1. ✅ **提升性能**: 利用.NET 8.0 LTS性能改进
2. ✅ **跨平台支持**: 确保在Windows、Linux、macOS上运行
3. ✅ **现代化工具链**: 支持最新IDE和开发工具

### 兼容性说明
- **向前兼容**: SDK同时支持.NET 8.0和.NET 10.0环境
- **企业友好**: 基于LTS版本，适合企业级应用部署
- **长期支持**: .NET 8.0 LTS支持至2026年11月

## 升级步骤规划

### 第一阶段：项目结构升级

#### 1.1 备份当前项目
```bash
git add .
git commit -m "备份：升级前的原始状态"
git tag -a "v3.5.1-original" -m "原始版本备份"
```

#### 1.2 创建新的SDK风格项目文件
- 移除旧的`YOP.SDK.csproj`
- 创建新的SDK风格项目文件
- 移除`packages.config`
- 更新依赖引用方式

#### 1.3 更新目标框架
- 从.NET Framework 4.8升级到.NET 8.0
- 确保API兼容性

### 第二阶段：依赖管理升级

#### 2.1 更新NuGet包
```xml
<!-- 新的PackageReference方式 -->
<PackageReference Include="BouncyCastle.Crypto" Version="1.8.9" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.Text.Encodings.Web" Version="8.0.0" />
```

#### 2.2 移除直接DLL引用
- 删除根目录下的DLL文件
- 清理bin/obj目录
- 统一使用NuGet包管理

#### 2.3 处理兼容性问题
- 检查BouncyCastle API变化
- 更新加密相关代码
- 处理System.Web依赖




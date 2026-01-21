# YOP .NET SDK

YOP .NET SDK 是易宝支付开放平台提供的官方 .NET SDK，用于简化商户与易宝支付平台的集成。本 SDK 基于 .NET 8.0 开发，支持 RESTful API 调用、数据加密、签名验证等功能。

## 概述

**版本信息**：4.0.0  
**支持框架**：.NET 8.0 LTS (长期支持版本)  
**最新版本密钥格式**：PKCS8格式（3.5.0及以后版本），3.5.0以前版本使用PKCS1格式

## 核心功能

- 支持多种HTTP请求方式（GET、POST、文件上传下载）
- 自动签名和加密处理
- 支持JSON数据格式
- 完善的异常处理机制
- 支持多应用配置
- 支持请求和响应加密

## 快速开始

### 1. 环境要求

- .NET 8.0 LTS 或更高版本
- Visual Studio 2022 或 Visual Studio Code

**注意**：本SDK基于.NET 8.0 LTS构建，同时兼容.NET 10.0环境。推荐使用.NET 8.0 LTS以获得最佳稳定性和长期支持。

### 2. 安装SDK

将 SDK 项目添加到您的解决方案中，或直接引用编译后的程序集。

### 3. 配置初始化

```csharp
using SDK.yop.client;

// 配置应用信息
YopConfig.setAppKey("YOUR_APP_KEY");           // 应用密钥
YopConfig.setAesSecretKey("YOUR_SECRET_KEY");  // 商户私钥

// 配置服务器地址
// 测试环境
YopConfig.setServerRoot("https://sandbox.yeepay.com/yop-center");
YopConfig.setYosServerRoot("https://sandbox.yeepay.com/yop-center");

// 生产环境
// YopConfig.setServerRoot("https://openapi.yeepay.com/yop-center");
// YopConfig.setYosServerRoot("https://yos.yeepay.com/yop-center");
```

### 4. 创建请求对象

```csharp
// 使用全局配置创建请求
var request = new YopRequest();

// 或指定应用信息创建请求
var request = new YopRequest("appKey", "secretKey", "yopPublicKey");
```

### 5. 添加请求参数

```csharp
// 添加普通参数
request.addParam("merchantNo", "YOUR_MERCHANT_NO");
request.addParam("orderId", "ORDER_123456");
request.addParam("orderAmount", "100.00");

// 添加文件参数（用于文件上传）
request.addFile("merQual", "/path/to/your/file.txt");

// 设置商户编号（可选）
request.setCustomerNo("YOUR_CUSTOMER_NO");

// 设置加密（可选）
request.setEncrypt(true);

// 设置返回结果签名验证（可选）
request.setSignRet(true);
```

## API调用示例

### 1. GET请求示例

```csharp
public void TestGetForm()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey, YopPublicKey);
        request.addParam("file", "test_file");
        request.addParam("fileFileSize", "1024");
        request.addParam("fileMd5", "d41d8cd98f00b204e9800998ecf8427e");
        request.addParam("fileOriginalFileName", "test.txt");
        request.addParam("fileBucket", "test-bucket");

        // 使用YopRsaClient发起请求
        var response = YopRsaClient.get("/rest/v1.0/lx-test/lx-getform-old", request);

        if (response != null && response.result != null)
        {
            Console.WriteLine($"GET Form API响应: {FormatJson(response.result.ToString())}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"GET Form API测试异常: {ex.Message}");
    }
}
```

### 2. POST Form请求示例

```csharp
public void TestPostForm()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey, YopPublicKey);
        request.addParam("file", "test_file");
        request.addParam("fileFileSize", "1024");
        request.addParam("fileMd5", "d41d8cd98f00b204e9800998ecf8427e");
        request.addParam("fileOriginalFileName", "test.txt");
        request.addParam("fileBucket", "test-bucket");

        // 使用YopRsaClient发起POST请求
        var response = YopRsaClient.post("/rest/v1.0/lx-test/lx-postform-old", request);

        if (response != null && response.result != null)
        {
            Console.WriteLine($"POST Form API响应: {FormatJson(response.result.ToString())}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"POST Form API测试异常: {ex.Message}");
    }
}
```

### 3. POST JSON请求示例

```csharp
public void TestPostJson()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey, YopPublicKey);

        var parameters = new Dictionary<string, object>
        {
            { "file", "test_file" },
            { "fileFileSize", "1024" },
            { "fileMd5", "d41d8cd98f00b204e9800998ecf8427e" },
            { "fileOriginalFileName", "test.txt" },
            { "fileBucket", "test-bucket" }
        };

        var jsonContent = JsonConvert.SerializeObject(parameters);
        request.setContent(jsonContent);

        // 使用YopRsaClient发起POST JSON请求
        var response = YopRsaClient.post("/rest/v1.0/lx-test/lx-postjson-old", request);

        if (response != null && response.result != null)
        {
            Console.WriteLine($"POST JSON API响应: {FormatJson(response.result.ToString())}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"POST JSON API测试异常: {ex.Message}");
    }
}
```

### 4. 文件上传示例

```csharp
public void TestUploadOldApi()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey, YopPublicKey);
        
        // 创建临时文件
        var tempFilePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFilePath, "hello world");
            request.addFile("merQual", tempFilePath);
            request.addParam("merQualOriginalFileName", "test.txt");
            
            // 使用YopRsaClient发起上传请求
            var response = YopRsaClient.upload("/yos/v1.0/test/mer-file-upload/upload/backupload", request);
            
            if (response != null && response.result != null)
            {
                Console.WriteLine($"上传API响应: {FormatJson(response.result.ToString())}");
            }
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"上传API测试异常: {ex.Message}");
    }
}
```

### 5. 文件下载示例

```csharp
public void TestDownloadOldApi()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey, YopPublicKey);
        request.addParam("fileName", "test.txt");

        // 使用YopRsaClient发起下载请求
        var response = YopRsaClient.get("/yos/v1.0/test/test/ceph-download", request);

        if (response != null && response.result != null)
        {
            Console.WriteLine($"下载API响应: {FormatJson(response.result.ToString())}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"下载API测试异常: {ex.Message}");
    }
}
```

## 高级功能

### 1. 数据加密

SDK 支持请求和响应数据的自动加密：

```csharp
// 启用请求加密
request.setEncrypt(true);

// 启用响应签名验证
request.setSignRet(true);
```

### 2. 签名验证

SDK 自动处理请求签名，并支持响应签名验证：

```csharp
// 检查响应签名是否有效
if (response.isValidSign())
{
    // 签名验证通过
}
else
{
    // 签名验证失败
}
```

### 3. 请求配置和签名功能测试

```csharp
public void TestRequestConfigAndSign()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey);
        request.addParam("testParam", "testValue");
        request.setEncrypt(true);
        request.setSignRet(true);

        // 验证请求参数
        request.getAppKey().Should().Be(AppKey);
        request.getSecretKey().Should().Be(SecretKey);
        request.isEncrypt().Should().BeTrue();
        request.isSignRet().Should().BeTrue();

        // 验证参数添加
        request.getParamValue("testParam").Should().Be("testValue");

        Console.WriteLine("请求配置和签名功能测试通过");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"请求配置和签名功能测试异常: {ex.Message}");
        throw;
    }
}
```

### 4. 响应处理功能测试

```csharp
public void TestResponseHandling()
{
    try
    {
        var request = new YopRequest(AppKey, SecretKey);
        var response = new YopResponse();

        // 设置响应数据
        response.state = "SUCCESS";
        response.result = "{\"test\":\"value\"}";
        response.ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        response.validSign = true;

        // 验证响应处理
        response.isSuccess().Should().BeTrue();
        response.isValidSign().Should().BeTrue();
        response.result.Should().NotBeNull();

        Console.WriteLine("响应处理功能测试通过");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"响应处理功能测试异常: {ex.Message}");
        throw;
    }
}
```

## 工具类使用

### 1. 签名-收银台

```csharp
public void TestSignForCashier()
{
    try
    {
        // 测试私钥
        string private_key = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDBHBdHbQXsPT+EpAhLA9k2Q5O8GLCAUFLWYB57Uhc4ZNa2YUhjrTFvFZMFQuMjaVgdmFGTvqfGYUQBRldHFhf9kuXf5LPb+m0BJ/R5AWCyTcX7DHouoGODMfxkCZrimILwYWDkwhYTHr5hEV58nGRQtHOIVB5a4i/y4Z1vvX1MIjA+8OJ3zpaxXKkj+46OtfmjloUPGFSzz+rqrRRtMqYePLkWZ0J+CmIXM1Kwl/kgYUq/YGSYy9Q5vTojN9WBKzk7euOoCcsWtRrBQysdyM3yDPXjhnXx7G0nh07hSUh+rMDZ7Zst0lVQol/7kPXzNwh6eUmRGY9lfruIMS5kg1ydAgMBAAECggEAD4yQf0rTCEOiQq7mkAu+SLVGRwYB6EMPeH2C1tE0V3EfLM5GgugmK9ij3u+U1HweATwLjYbzgXDBhgzA6FNqGRvj8JQ8u0C92DL8Z2XqAFFs2JsXl3uIp761oOR5GTfIi0x7/c928ZEvKSe54PTCyxDMoLSNQSonTDpIb//k/+U4xEOQ1mjlSvlOM5ic7/kdw+G+aP/Hk/T6kg/vIblWQHx8SB3WYpLb/R6oPO+05X+zcQ+vVX1TrQ/amDp6/PouWjTF5hf48JEBdM8+xJzUwnalrG9U7pChfyGAOXQT1fbDdywBJXt6pZsT/mz1RkUC5Uto5/aVQGIDD+IPm/ZDbQKBgQDEwF09sjUb5hHEdmG28RmMf4E0JCOEzCvxiUpovobymqapLM5bf2oLNXqGenEAMbfFQatJFVKx6YBZwFIj/xzQJt8fL/jRzlbLijaANP+1JacvTsfXKBXS888FN3rkKisTlhmYXI+4EwA1wbcRkLDH3vezdVCi9cszQ9HvwkVfOwKBgQD7QvzD3pirXIJ64JizWTS4MJMko3CWepsq9UZ5uyoHWh7tSz86H/2y0FK10YpJEJeGtyPXlnU+uQwjYMJRPLlNv8180pjCJX2ZTW2drB2vOJvormhMhDIYAZtPAHu2dajzdy4VRuvFTtH4FpW/KjAJrTLK3ze3K95ACYVBJ8EmBwKBgB0K8DiNN724hmLjvqTMjiLpJ19U/lE5+jqbM3qmtTDWl0ddr9BdzH9/E2kKZefLbv8VJH2TQjO07hdRhk599/jZ5BGseSQvOyysaEMgj6ZjunwHOwSNjDspdiOk/uTzPIyVmY2eDDD1zRAiWi2jmBTI2vOIm7CSa75TgofLu4XFAoGBAJrFM4+vYNlFXbY0/LqU+21ttmV+K471rPj0Jto7GPN4Zs6CaEr0g8COpDQNA6JoDv5Td0eIDWZ6c+ii5G9H+VjUCc6WprQIhepVkGzsJUjWlOrp66MeVwEElFdAk/PbXBvEUOWYTwi1uY6Y0trzMK31OvFOODKjWf6WHrf4tfgnAoGAOri6bX2D/zqpJT3mJ5MIVJJbn4D4Idx+TCUaVRSY1rBp+Y2ofW1W8ktu7xPO9/LwVQR7kJeosEBAFGTmGqll033ywu5+8X8J1bw6HCghkI0yHW752sOdfl30kXi3Ds8tQsvSEHRfnPb8yvWve2srZb9ubwOvpI0PtOIujZP4fYI=";
        
        // 待签名原文
        string plainText = "这里是待签名原文呀123！@¥%……&*。。";
        
        // 使用SHA1withRSA进行签名
        string signature = SHA1withRSA.signRsa(plainText, private_key);
        
        // 验证签名不为空
        signature.Should().NotBeNullOrEmpty();
        
        Console.WriteLine($"签名原文: {plainText}");
        Console.WriteLine($"签名结果: {signature}");
        Console.WriteLine("签名功能测试通过");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"签名功能测试异常: {ex.Message}");
        throw;
    }
}
```

### 2. 通知解密

```csharp
public void TestNoticeDecrypt()
{
    try
    {
        // 这是接收到的通知报文
        var callContent = "response=ZIcrArlonH0mxIWCRejL2VQS2qeK5EFz2gALzdMbusIU8eqwnWNgJRWiTwJElSQEhnT42KkU3jXWZr2dd0A8-bZSjT-hvNCUI0aoJZkadRtJrWoe_ygGhOLegj7cTbk8y7GOzfFQteIFbB9ALae1CqWVHgfgyozbTLgsse4MfuYjio9r3DOkCJJSkW6mEHB0G4rTXSWFni0h_Uhtu5jsuCTU4vWDPKrBIZI17rr1AIqmyOd8C8oLCAplC1JT4KnLq5QCir4cnvZJrGYB5-bI00gPOdGX2_v4Az3VqMkh8PqqPSDriJ-PqDo9T2dnjR5njYkTSSzUpIXg6cfhLaTNIQ$0sn1fsX0zRYXv-bb0tV531Brbhb-fPORrXYqe8JzHbnL8NkAwIPRkSaTXfq3etJnmslkkBlPpZeTc7639TWQlBrl3eVW-aQKIjFX4bhfyythIh5ByjBAHw1RaYwoHw10kkpbBBk01K-6pzE9QzT6TvjZLsSsXZ6O3WJdvrB8dtpJA-PI-sOzm7DXkBqfKOSufkN1C1mRvexBlcN3ScSH2TKo5ZwKw3Fo_93GsYFD0hzYmHpC6yCyHXeY1PPlHYqd_KsqXVo_xBtXMCadoKnldYnMljXdhAQJLRdlkwTgeD8FX18SQSJ18O6Ag0w3IM9QXkcgZVgIo1-_ZUncc5AtNXyQCvfT4tNyaIRFsXlFqj5tCc5bekMz8OzYeRTPfmfCLKXmjvg4ICMw0aIRboX1tyZpCHdHU269u0-wX90pMDNRqBZsLag6glNDSzEG8RQaB4vGrjvxYy0ixeUnogwni2qqnnGX5Gfhkst7FPYubAsi5HweDT_aJIrmE6kMiBrpMAOcIGZ6slYK854FOH3ODO9-raz7n2P__NUTpziTF4t4Jru_erJevVoGyHH81qq_msIMvK7IRx2z1QoExRL08A$AES$SHA256&customerIdentification=app_Fe51qCyZWcEnDMtK";
        
        // 获取 response 值 并 Url解码
        var decodeData = System.Web.HttpUtility.UrlDecode(callContent.Split('&').FirstOrDefault(p => p.StartsWith("response")).Replace("response=", ""));
        
        string isvPriKey = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCFqza8OmK0drABjmphX3erNEoLJ6kCqaE/AWvqEReAq5K8+3btAVEJYBiMbEJUvZpV4YCITDhyJeRzdfLl2EGnsvI4lrhvrvGAjW9lB34b7GdJSMFFj+ExrTuCQ7zDeUgATlmhLyHxWZ6IAVwf1a3G5bk2GhUipFc0dL8HviVk25CSAgCWKL8LZ0V4ec+FF3/Z6orJUChQOuvpNlclH65pHbdQ4Mr3rQUI0z7WSaW1U1EZeXSctRPf/3JQYLi+RW/yogXG8t+6zn9jE/tGuWJV0bm/ZUDU2XydjZgU0E/NzMutmNVSSKpBy74vt5SCNXkK0BgjRTXP8HGvEgA7IYfLAgMBAAECggEAQNlOxcUBrBHE1Ax22eTKFvpYTc8g9NS9EOcsprNCFr+mgh7xlIxF92lyn3XKPHh8DtxHUljALcjqa4W2oQHo4GY1k3Sz6CMUsUxs1bPr37oyZeBxO8FQ/JvRuiIIy0DkyJk6bLOEISZcfhlCy4MMOumqkG/Y/ySB1kYpg6UhWStlGdQZbHEL7NUBW1QVQqaDAtSV8cgNNXbinQ08pIMlfqnH57DM8J7lx+687li41ZkQgCc/r/zkV5Tf5ABlhDxmZZbHmU7jsvlqh6zxm12r7A0gOwnb/wVd0ZrpIE2TfsDGOQckfEmaD4uhsabDAVO7I6IDM86hH3Sm285hkQ/2eQKBgQC6eNRpJQv6TfXMqEoNMf503WGskWF0RFSU0mU/5dSX8GELhLkyAtAhZFnrOKcseZJng/Z6grfS2wIG3Y17kXIlbi8Wprk0F9AsUOSEFPFRW/I7y7aUDvGf8Jz/TsiYEatSE0SJCWZbpaO8+WoczNybWNyyHapq2g6+xHSlyEp9nQKBgQC3gjOIF1dt67lrWSogVDX1eEpp6yLxKXC0usGV/0sB2FK5S9KqEh2Ul+rWwlxXxLpVYHhHFr1k7PyFrnwsTUxNHj2joCD1yOhMoxgMoRm0/z43cV/0TeTcLTQkxdCtwZ6pQwHIAHenLRpxpnD8Y8EplA3VS9Lno7eSC9VjqgFShwKBgF83vfcm1LPuxTnJIW8VfUK9nNeasPHGxo3r1YnIWUNwmo1gK5UO/KpgbM4A8tRyC8FSEDVEtIs2DBXnYgycG3ZjiiX94opoMoO+lsGfVA5gbhP8lPGLo/Qw0GpKF4IXW60ga5myNBNORIsFrRqhvXCR8rf9D/1Z9beR56KT4P29AoGAV986+dHjhbk4wpShvXVVmUOOroVv5/cmBwTeqgrjSfDiO+R47gNassrEIy5StZx4dWWKctAKxQdOLF1PDI+/F7aBYZbN8aPQyNHYNEP4YVlP25Col/2st1nV/D3VHT730KlLcw/2O9E3NnCy7ch+uIAy145FYbJdtst/1QeVNoUCgYEAtv3w9xNugsYl4/yNkC/DIXat9u/56sVjnYzxvwHZT8jtg4uo3qqlRqqm4OcUmzcsYodrsbn8upizq9ZS2NVdPrGIPFOZBgNUKWL1Ok/dJgfBcdpo72/UX4+KQ2/9c1ZrjjMs4sglsrzvZTXqkryXkPKxKk1EcjDaKiOExFnk6Y8=";
        string yopPubKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4g7dPL+CBeuzFmARI2GFjZpKODUROaMG+E6wdNfv5lhPqC3jjTIeljWU8AiruZLGRhl92QWcTjb3XonjaV6k9rf9adQtyv2FLS7bl2Vz2WgjJ0FJ5/qMaoXaT+oAgWFk2GypyvoIZsscsGpUStm6BxpWZpbPrGJR0N95un/130cQI9VCmfvgkkCaXt7TU1BbiYzkc8MDpLScGm/GUCB2wB5PclvOxvf5BR/zNVYywTEFmw2Jo0hIPPSWB5Yyf2mx950Fx8da56co/FxLdMwkDOO51Qg3fbaExQDVzTm8Odi++wVJEP1y34tlmpwFUVbAKIEbyyELmi/2S6GG0j9vNwIDAQAB";
        
        // 使用SHA1withRSA进行通知解密
        string cipherText = SHA1withRSA.NoticeDecrypt(decodeData, isvPriKey, yopPubKey);
        
        // 验证解密结果不为空
        cipherText.Should().NotBeNullOrEmpty();
        
        Console.WriteLine($"解密结果: {cipherText}");
        Console.WriteLine("通知解密功能测试通过");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"通知解密功能测试异常: {ex.Message}");
        throw;
    }
}
```

### 3. AES加密解密工具

```csharp
public void TestEncryptDecrypt()
{
    try
    {
        var originalText = "test encryption data";
        var aesKey = "1234567890123456"; // 16位密钥

        // 测试AES加密解密
        var encrypted = AESHelper.AESEncrypt(originalText, aesKey);
        var decrypted = AESHelper.AESDecrypt(encrypted, aesKey);

        decrypted.Trim().Should().Be(originalText);

        Console.WriteLine("加密解密功能测试通过");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"加密解密功能测试异常: {ex.Message}");
        throw;
    }
}
```

### 4. JSON格式化工具

```csharp
/// <summary>
/// 格式化JSON字符串以便更好地显示
/// </summary>
/// <param name="json">JSON字符串</param>
/// <returns>格式化后的JSON字符串</returns>
private string FormatJson(string json)
{
    try
    {
        var parsedObject = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(parsedObject, Formatting.Indented);
    }
    catch
    {
        return json;
    }
}
```

## 测试用例

### 综合测试方法

```csharp
[Fact]
public void TestAllApis()
{
    // 测试下载API
    TestDownloadOldApi();

    // 测试GET Form API
    TestGetForm();

    // 测试POST Form API
    TestPostForm();

    // 测试POST JSON API
    TestPostJson();

    // 测试上传API
    TestUploadOldApi();
}
```

## 最佳实践

1. **密钥安全**：妥善保管您的密钥，不要在代码中硬编码，建议使用配置文件或环境变量。

2. **异常处理**：始终使用try-catch块处理SDK调用，确保程序稳定运行。

3. **超时设置**：根据网络环境合理设置超时时间，避免长时间等待。

4. **日志记录**：记录请求和响应信息，便于问题排查。

5. **签名验证**：对于重要业务，建议启用响应签名验证，确保数据完整性。

6. **重试机制**：对于网络不稳定的情况，可以实现适当的重试机制。

7. **文件处理**：上传文件后及时清理临时文件，避免占用磁盘空间。

## 环境配置

### 测试环境配置

```csharp
// 测试环境，商户私钥
private const string AppKey = "app_100800191870146";
private const string SecretKey = "YOUR_SECRET_KEY";
// 测试环境，平台公钥
private const string YopPublicKey = "YOUR_PUBLIC_KEY";
// 测试环境，请求端点地址
private const string ServerRoot = "https://sandbox.yeepay.com/yop-center";
private const string YosServerRoot = "https://sandbox.yeepay.com/yop-center";
```

### 生产环境配置

```csharp
// 生产环境，平台公钥
// private const string YopPublicKey = "YOUR_PUBLIC_KEY";
// 生产环境，请求端点地址
// private const string ServerRoot = "https://openapi.yeepay.com/yop-center";
// private const string YosServerRoot = "https://yos.yeepay.com/yop-center";
```

## 常见问题

### Q: 如何切换测试环境和生产环境？
A: 通过设置不同的服务器地址：
```csharp
// 测试环境
YopConfig.setServerRoot("https://sandbox.yeepay.com/yop-center");
YopConfig.setYosServerRoot("https://sandbox.yeepay.com/yop-center");

// 生产环境
YopConfig.setServerRoot("https://openapi.yeepay.com/yop-center");
YopConfig.setYosServerRoot("https://yos.yeepay.com/yop-center");
```

### Q: 如何处理大文件上传？
A: SDK支持文件上传，但对于大文件建议分片上传或使用断点续传。

### Q: 如何验证响应签名？
A: 设置`request.setSignRet(true)`，SDK会自动验证响应签名，可通过`response.isValidSign()`检查结果。

### Q: 如何处理网络超时？
A: 合理设置连接超时和读取超时时间，并实现重试机制。

### Q: 如何添加JSON内容到请求？
A: 使用`request.setContent(jsonContent)`方法添加JSON内容。

## 技术支持

如有问题，请联系易宝支付技术支持或查阅官方文档。

---

*本文档基于YOP .NET SDK v4.0.0版本编写，最新信息请参考官方文档。有关版本更新详情，请参阅 [CHANGELOG.md](./CHANGELOG.md)。*
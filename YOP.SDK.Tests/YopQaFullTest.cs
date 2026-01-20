using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SDK.yop.client;
using SDK.yop.utils;
using SDK.yop.encrypt;
using SDK.enums;
using Xunit;
using FluentAssertions;

namespace YOP.SDK.Tests
{
    /// <summary>
    /// YOP SDK 完整测试类，覆盖Java单元测试中的所有场景
    /// </summary>
    public class YopQaFullTest
    {
        // 测试环境，商户私钥
        private const string AppKey = "app_100800191870146";

        private const string SecretKey =
            "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCcl9jdReLN2vk57DWeu2l223gO6txH4ra/cOkM1rHYSwUqrvVYUM61qNL4AZNO3nlBMUkEtZ5RyvBGajJ0OZidenqcnXHKTBOD6GcVpBsS4qCU5hIHspI0khYBhikl1z74E3HHe9sIwVz+gEx2TkCwH9aH4jh68YeKk25+tHeb75JPMhdj0S+IzWyNHFOOTPD6VQ3vCq1RDqutL56gVBIt6p4+ZQ4GkTDRQoD7OKWDZOkzXz+JnaLqIN/hnZ/8ag+0WY60rgpjXBAX1ezZtHGp33L4sqnIZIzIp38e1Oyfe+Ab1cBQkaRaBqKYRXjq7NXG0sfeuoFxJONPQfjSsYcxAgMBAAECggEABAJAJ0QBfycer7ysD7z5AXWzYGBjVMTJTGPZ134Mjf63qmTRu5nP/OcORZKWwJNh88kM9z2mCK6DEa5ozcBmt4tZ5bYDInxpmHwj3XI2zjg2h7FPH1rTMtzViuLyHTmiL0wiIsr5K8N1e79xlarBrbCW96ITM5SI1YOaNcytbjS82Y0b6AS3Jx4H02ihcpzFRPYZNffLhE3lu4WN6aoiQUnWvC+agwF88osHmUIwrKlo9unwzjzOh2bXGSvzTESeGdDMEz4ekNRX8dEfC31sJwd2tUqha5hTev/oFgymNwFhQvj0OfnB1oKjf7gaRbOYjFf5P3nou5d4EBR7OyyTpQKBgQC6pXRqwpVh8Hhvf0/vFYZk5yQsKBi/ik4vyYtIZVtdZGIq4vjUpIftt9WoxTYKhjgOL0vsPvtbR79XnMFOb0PSZXARfMoUlAwtMM+mk9N3fb5a6jrgV617Ob1Oqf691WPqdprb6t1HMBSYTzD3J1mPKeGmItsTAlFZuUjzkyvN3QKBgQDWx52jTXvc4QyOqu4Fc0mPSDJEYBBB6XpnTXRIT8rUFD9SDTWRhb29UL2nrtoQ6/8ZTC+/wKQ5B9UAUEwNxSk8IsrWDhcuioxOc1rT7nPRoVXQarr35//NV1CPmZyn6ybpCqDTsy6mvtWLIJdjZCc+1yZevM1uvOtRMrWRhs8bZQKBgEqIpfu4JqVMvRtxUL9d7iQ/NW+4t2FN3rkwl8FaUGj0HEuaBdoMtgdVASp7ToBXZu0rL/twjzm9ZgibnYov3nqXbXBeT+h10oL9Wf7gS3MNMMXngYlzGeD6hsFyGzs9ir/niyHFIYY7Cg5kmV4pRZdpFyYcBzYJF+lnl11FaRm1AoGAFu1wMoKO+mE7ye8NQZ+w9o6qbwoiMicOXgCyrRV3fXQ73jJyyXoRayg3VrMfrDbFIJo1bq7N2Riw8DuiIsYtRLIiHP+cEefQWn+N7pnB21rxojICi3xEnlL30px/UJ2VpcLwsCisjjhI63UrM/z5A4hMHEjjVTLtm9lh8IsHiNECgYBf+YEmhG88617fUMa7p9VLcwZNXWOgAFX230CYBFQQIgDuwINASoM4GGVfnvLPym4+WNW3cvfI1sNSm3VXzoM+uSHZtQUkG0SjHZl3buNGXXaJ1iZK/EoRHGraukC6ZuaDsnnjFQbRqvdZIFXqEWFbPFSWBQm9g2dYSbzNu3u9Kg==";

        // 测试环境，平台公钥
        private const string YopPublicKey =
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4g7dPL+CBeuzFmARI2GFjZpKODUROaMG+E6wdNfv5lhPqC3jjTIeljWU8AiruZLGRhl92QWcTjb3XonjaV6k9rf9adQtyv2FLS7bl2Vz2WgjJ0FJ5/qMaoXaT+oAgWFk2GypyvoIZsscsGpUStm6BxpWZpbPrGJR0N95un/130cQI9VCmfvgkkCaXt7TU1BbiYzkc8MDpLScGm/GUCB2wB5PclvOxvf5BR/zNVYywTEFmw2Jo0hIPPSWB5Yyf2mx950Fx8da56co/FxLdMwkDOO51Qg3fbaExQDVzTm8Odi++wVJEP1y34tlmpwFUVbAKIEbyyELmi/2S6GG0j9vNwIDAQAB";
        // 生产环境，平台公钥
        // private const string YopPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA6p0XWjscY+gsyqKRhw9MeLsEmhFdBRhT2emOck/F1Omw38ZWhJxh9kDfs5HzFJMrVozgU+SJFDONxs8UB0wMILKRmqfLcfClG9MyCNuJkkfm0HFQv1hRGdOvZPXj3Bckuwa7FrEXBRYUhK7vJ40afumspthmse6bs6mZxNn/mALZ2X07uznOrrc2rk41Y2HftduxZw6T4EmtWuN2x4CZ8gwSyPAW5ZzZJLQ6tZDojBK4GZTAGhnn3bg5bBsBlw2+FLkCQBuDsJVsFPiGh/b6K/+zGTvWyUcu+LUj2MejYQELDO3i2vQXVDk7lVi2/TcUYefvIcssnzsfCfjaorxsuwIDAQAB";

        // 测试环境，请求端点地址: 普通接口
        private const string ServerRoot = "https://sandbox.yeepay.com/yop-center";
        // 测试环境，请求端点地址: 文件类接口(上传/下载，/yos/开头)
        private const string YosServerRoot = "https://sandbox.yeepay.com/yop-center";
        
        // 生产环境，请求端点地址
        // private const string ServerRoot = "https://openapi.yeepay.com/yop-center";
        // private const string YosServerRoot = "https://yos.yeepay.com/yop-center";


        public YopQaFullTest()
        {
            // 初始化YOP客户端配置
            YopConfig.setAppKey(AppKey);
            YopConfig.setAesSecretKey(SecretKey);
            YopConfig.setServerRoot(ServerRoot);
            YopConfig.setYosServerRoot(YosServerRoot);
        }

        /// <summary>
        /// 测试下载API - 基于download-old-api.json
        /// </summary>
        [Fact]
        public void TestDownloadOldApi()
        {
            try
            {
                var request = new YopRequest(AppKey, SecretKey, YopPublicKey);
                request.addParam("fileName", "test.txt");

                // 使用YopRsaClient发起请求
                var response = YopRsaClient.get("/yos/v1.0/test/test/ceph-download", request);

                response.Should().NotBeNull();
                response.result.Should().NotBeNull();

                Console.WriteLine($"下载API响应: {FormatJson(response.result.ToString())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载API测试异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 测试GET Form API - 基于getform.json
        /// </summary>
        [Fact]
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

                response.Should().NotBeNull();
                response.result.Should().NotBeNull();

                Console.WriteLine($"GET Form API响应: {FormatJson(response.result.ToString())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GET Form API测试异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 测试POST Form API - 基于postform.json
        /// </summary>
        [Fact]
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

                // 使用YopRsaClient发起请求
                var response = YopRsaClient.post("/rest/v1.0/lx-test/lx-postform-old", request);

                response.Should().NotBeNull();
                response.result.Should().NotBeNull();

                Console.WriteLine($"POST Form API响应: {FormatJson(response.result.ToString())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"POST Form API测试异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 测试POST JSON API - 基于postjson.json
        /// </summary>
        [Fact]
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

                // 使用YopRsaClient发起请求
                var response = YopRsaClient.post("/rest/v1.0/lx-test/lx-postjson-old", request);

                response.Should().NotBeNull();
                response.result.Should().NotBeNull();

                Console.WriteLine($"POST JSON API响应: {FormatJson(response.result.ToString())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"POST JSON API测试异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 测试上传API - 基于upload-old-api.json
        /// </summary>
        [Fact]
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
                    // 使用YopRsaClient发起请求
                    var response = YopRsaClient.upload("/yos/v1.0/test/mer-file-upload/upload/backupload", request);
                    response.Should().NotBeNull();
                    response.result.Should().NotBeNull();
                    Console.WriteLine($"上传API响应: {FormatJson(response.result.ToString())}");
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
                throw;
            }
        }

        /// <summary>
        /// 综合测试方法 - 结合所有API类型
        /// </summary>
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

        /// <summary>
        /// 测试请求配置和签名功能
        /// </summary>
        [Fact]
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

        /// <summary>
        /// 测试响应处理功能
        /// </summary>
        [Fact]
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

        /// <summary>
        /// 测试加密解密功能
        /// </summary>
        [Fact]
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

        /// <summary>
        /// 测试签名功能 - 收银台
        /// </summary>
        [Fact]
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

        /// <summary>
        /// 测试通知解密功能
        /// </summary>
        [Fact]
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
    }
}
using System.Collections;
using System.Reflection;
using FluentAssertions;
using SDK.yop.client;
using Xunit;

namespace YOP.SDK.C.Tests
{
    /// <summary>
    /// customerNo 作为业务参数与鉴权参数的兼容测试
    /// </summary>
    public class YopRsaCustomerNoTest
    {
        private const string AppKey = "app_100800191870146";
        private const string SecretKey =
            "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCcl9jdReLN2vk57DWeu2l223gO6txH4ra/cOkM1rHYSwUqrvVYUM61qNL4AZNO3nlBMUkEtZ5RyvBGajJ0OZidenqcnXHKTBOD6GcVpBsS4qCU5hIHspI0khYBhikl1z74E3HHe9sIwVz+gEx2TkCwH9aH4jh68YeKk25+tHeb75JPMhdj0S+IzWyNHFOOTPD6VQ3vCq1RDqutL56gVBIt6p4+ZQ4GkTDRQoD7OKWDZOkzXz+JnaLqIN/hnZ/8ag+0WY60rgpjXBAX1ezZtHGp33L4sqnIZIzIp38e1Oyfe+Ab1cBQkaRaBqKYRXjq7NXG0sfeuoFxJONPQfjSsYcxAgMBAAECggEABAJAJ0QBfycer7ysD7z5AXWzYGBjVMTJTGPZ134Mjf63qmTRu5nP/OcORZKWwJNh88kM9z2mCK6DEa5ozcBmt4tZ5bYDInxpmHwj3XI2zjg2h7FPH1rTMtzViuLyHTmiL0wiIsr5K8N1e79xlarBrbCW96ITM5SI1YOaNcytbjS82Y0b6AS3Jx4H02ihcpzFRPYZNffLhE3lu4WN6aoiQUnWvC+agwF88osHmUIwrKlo9unwzjzOh2bXGSvzTESeGdDMEz4ekNRX8dEfC31sJwd2tUqha5hTev/oFgymNwFhQvj0OfnB1oKjf7gaRbOYjFf5P3nou5d4EBR7OyyTpQKBgQC6pXRqwpVh8Hhvf0/vFYZk5yQsKBi/ik4vyYtIZVtdZGIq4vjUpIftt9WoxTYKhjgOL0vsPvtbR79XnMFOb0PSZXARfMoUlAwtMM+mk9N3fb5a6jrgV617Ob1Oqf691WPqdprb6t1HMBSYTzD3J1mPKeGmItsTAlFZuUjzkyvN3QKBgQDWx52jTXvc4QyOqu4Fc0mPSDJEYBBB6XpnTXRIT8rUFD9SDTWRhb29UL2nrtoQ6/8ZTC+/wKQ5B9UAUEwNxSk8IsrWDhcuioxOc1rT7nPRoVXQarr35//NV1CPmZyn6ybpCqDTsy6mvtWLIJdjZCc+1yZevM1uvOtRMrWRhs8bZQKBgEqIpfu4JqVMvRtxUL9d7iQ/NW+4t2FN3rkwl8FaUGj0HEuaBdoMtgdVASp7ToBXZu0rL/twjzm9ZgibnYov3nqXbXBeT+h10oL9Wf7gS3MNMMXngYlzGeD6hsFyGzs9ir/niyHFIYY7Cg5kmV4pRZdpFyYcBzYJF+lnl11FaRm1AoGAFu1wMoKO+mE7ye8NQZ+w9o6qbwoiMicOXgCyrRV3fXQ73jJyyXoRayg3VrMfrDbFIJo1bq7N2Riw8DuiIsYtRLIiHP+cEefQWn+N7pnB21rxojICi3xEnlL30px/UJ2VpcLwsCisjjhI63UrM/z5A4hMHEjjVTLtm9lh8IsHiNECgYBf+YEmhG88617fUMa7p9VLcwZNXWOgAFX230CYBFQQIgDuwINASoM4GGVfnvLPym4+WNW3cvfI1sNSm3VXzoM+uSHZtQUkG0SjHZl3buNGXXaJ1iZK/EoRHGraukC6ZuaDsnnjFQbRqvdZIFXqEWFbPFSWBQm9g2dYSbzNu3u9Kg==";
        private const string CustomerNo = "10012426723";

        [Fact]
        public void RemoveProtectedParams_ShouldKeepCustomerNo_WhenAppKeyPresent()
        {
            var request = new YopRequest(AppKey, SecretKey);
            request.addParam("customerNo", CustomerNo);

            InvokeRemoveProtectedParams(request);

            request.getParamValue("customerNo").Should().Be(CustomerNo);
            request.getParamValue("appKey").Should().BeNullOrEmpty();
        }

        [Fact]
        public void RemoveProtectedParams_ShouldStripCustomerNo_WhenAppKeyAbsent()
        {
            var request = new YopRequest(string.Empty, SecretKey);
            request.setCustomerNo(CustomerNo);

            InvokeRemoveProtectedParams(request);

            request.getParamValue("customerNo").Should().BeNullOrEmpty();
        }

        [Fact]
        public void Sign_ShouldUseAppKeyHeader_WhenAppKeyAndCustomerNoBothPresent()
        {
            var request = new YopRequest(AppKey, SecretKey);
            request.addParam("customerNo", CustomerNo);
            request.setCustomerNo(CustomerNo);

            var headers = InvokeSignRsaParameter(request);

            headers.ContainsKey("x-yop-appkey").Should().BeTrue();
            headers["x-yop-appkey"].Should().Be(AppKey);
            headers.ContainsKey("x-yop-customerid").Should().BeFalse();
        }

        [Fact]
        public void Sign_ShouldUseCustomerIdHeader_WhenOnlyCustomerNoPresent()
        {
            var request = new YopRequest(string.Empty, SecretKey);
            request.setCustomerNo(CustomerNo);

            var headers = InvokeSignRsaParameter(request);

            headers.ContainsKey("x-yop-customerid").Should().BeTrue();
            headers["x-yop-customerid"].Should().Be(CustomerNo);
            headers.ContainsKey("x-yop-appkey").Should().BeFalse();
        }

        private static void InvokeRemoveProtectedParams(YopRequest request)
        {
            typeof(YopRsaClient)
                .GetMethod("RemoveProtectedParamsForRsa", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { request });
        }

        private static Hashtable InvokeSignRsaParameter(YopRequest request)
        {
            request.setMethod("/rest/v1.0/test/demo");
            return (Hashtable)typeof(YopRsaClient)
                .GetMethod("SignRsaParameter", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { "/rest/v1.0/test/demo", request, "POST" });
        }
    }
}

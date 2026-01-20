using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using SDK.common;
using SDK.yop.client;

namespace SDK.yop.utils
{
    public class HttpUtils
    {
        /// <summary>
        /// The Set of accepted and valid Url characters per RFC3986.
        /// Characters outside of this set will be encoded.
        /// </summary>
        public const string ValidUrlCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        /// <summary>
        /// The set of accepted and valid Url path characters per RFC3986.
        /// </summary>
        private static string ValidPathCharacters = DetermineValidPathCharacters();

        private static readonly HttpClient SharedClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            // 兼容历史行为：忽略证书错误（不建议生产环境这么做，但为了不破坏现有 SDK 行为先保留）。
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.All,
                SslOptions =
                {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                }
            };

            return new HttpClient(handler, disposeHandler: true)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        // Checks which path characters should not be encoded
        // This set will be different for .NET 4 and .NET 4.5, as
        // per http://msdn.microsoft.com/en-us/library/hh367887%28v=vs.110%29.aspx
        private static string DetermineValidPathCharacters()
        {
            const string basePathCharacters = "/:'()!*[]$";

            var sb = new StringBuilder();
            foreach (var c in basePathCharacters)
            {
                var escaped = Uri.EscapeDataString(c.ToString());
                if (escaped.Length == 1 && escaped[0] == c)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static HttpResponseMessage Send(YopRequest yopRequest, string method, Hashtable headers = null)//(string targetUrl, string param, string method, int timeOut)
        {
            if (yopRequest == null)
            {
                throw new ArgumentNullException(nameof(yopRequest));
            }

            string targetUrl = yopRequest.getAbsoluteURL();//请求地址
            string param = yopRequest.toQueryString();//请求参数

            string methodUpper = (method ?? string.Empty).Trim().ToUpperInvariant();
            if (methodUpper == "GET")
            {
                targetUrl = targetUrl + (param.Length == 0 ? "" : ("?" + param));
            }

            // 兼容历史行为：上传逻辑传入 PUT，但底层实际发 POST multipart
            HttpMethod httpMethod = methodUpper == "PUT" ? HttpMethod.Post : new HttpMethod(methodUpper);
            var requestMessage = new HttpRequestMessage(httpMethod, targetUrl);

            ApplyHeaders(requestMessage, headers);

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            requestMessage.Headers.UserAgent.Clear();
            requestMessage.Headers.UserAgent.ParseAdd(".NET/" + YopConfig.getSdkVersion());

            if (methodUpper == "POST")
            {
                if (StringUtils.hasText(yopRequest.getContent()))
                {
                    requestMessage.Content = new StringContent(yopRequest.getContent(), Encoding.UTF8, "application/json");
                }
                else
                {
                    requestMessage.Content = new StringContent(param ?? string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");
                }
            }
            else if (methodUpper == "PUT")
            {
                requestMessage.Content = BuildMultipartFormDataContent(yopRequest, param);
            }

            int timeoutMs = yopRequest.getReadTimeout();
            using (var cts = new CancellationTokenSource(timeoutMs <= 0 ? 60000 : timeoutMs))
            {
                // 不调用 EnsureSuccessStatusCode，保持“错误响应也能读 body”的历史行为
                return SharedClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            }
        }

        private static void ApplyHeaders(HttpRequestMessage requestMessage, Hashtable headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (object k in headers.Keys)
            {
                string key = k?.ToString();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                string value = headers[k]?.ToString() ?? string.Empty;
                requestMessage.Headers.TryAddWithoutValidation(key, value);
            }
        }

        private static MultipartFormDataContent BuildMultipartFormDataContent(YopRequest yopRequest, string param)
        {
            var multipart = new MultipartFormDataContent();

            // 1) key/value（沿用旧逻辑：直接从 toQueryString() 拆分，不做 URL decode）
            if (!string.IsNullOrEmpty(param))
            {
                ArrayList aryParam = new ArrayList(param.Split('&'));
                for (int i = 0; i < aryParam.Count; i++)
                {
                    string a = (string)aryParam[i];
                    if (string.IsNullOrEmpty(a))
                    {
                        continue;
                    }
                    int n = a.IndexOf("=");
                    if (n <= 0)
                    {
                        continue;
                    }
                    string key = a.Substring(0, n);
                    string value = a.Substring(n + 1);
                    if (key.Equals("_file"))
                    {
                        continue;
                    }
                    multipart.Add(new StringContent(value ?? string.Empty, Encoding.UTF8), key);
                }
            }

            // 2) file：兼容旧逻辑 _file + files map
            string filePath = yopRequest.getParamValue("_file");
            if (!StringUtils.isBlank(filePath) && File.Exists(filePath))
            {
                var fileStream = File.OpenRead(filePath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                multipart.Add(fileContent, "_file", Path.GetFileName(filePath));
            }

            Dictionary<string, string> files = yopRequest.getFiles();
            if (files != null)
            {
                foreach (string key in files.Keys)
                {
                    string path = files[key];
                    if (StringUtils.isBlank(path) || !File.Exists(path))
                    {
                        continue;
                    }
                    var fileStream = File.OpenRead(path);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipart.Add(fileContent, key, Path.GetFileName(path));
                }
            }

            return multipart;
        }

        public static HttpResponseMessage SendMultipart(YopRequest yopRequest, IEnumerable<UploadFile> files, Hashtable headers = null)
        {
            if (yopRequest == null)
            {
                throw new ArgumentNullException(nameof(yopRequest));
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, yopRequest.getAbsoluteURL());
            ApplyHeaders(requestMessage, headers);

            var multipart = new MultipartFormDataContent();

            NameValueCollection values = yopRequest.getParams();
            if (values != null)
            {
                foreach (string key in values.Keys)
                {
                    multipart.Add(new StringContent(values[key] ?? string.Empty, Encoding.UTF8), key);
                }
            }

            if (files != null)
            {
                foreach (UploadFile file in files)
                {
                    if (file == null)
                    {
                        continue;
                    }
                    var content = new ByteArrayContent(file.Data ?? Array.Empty<byte>());
                    content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                    multipart.Add(content, file.Name, file.Filename);
                }
            }

            requestMessage.Content = multipart;

            int timeoutMs = yopRequest.getReadTimeout();
            using (var cts = new CancellationTokenSource(timeoutMs <= 0 ? 60000 : timeoutMs))
            {
                return SharedClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            }
        }

        /**
        * @param $path
        * @return string
        */
         public static string getCanonicalURIPath(string path)
        {
            if (path == null)
            {
                return "/";
            }
            else if (path.StartsWith("/"))
            {
                return normalizePath(path);
            }
            else
            {
                return "/" + normalizePath(path);
            }
        }

        public static string normalizePath(string path)
        {
            return normalize(path).Replace("%2F", "/");
        }

        /**
        * @param $value        * @return string
        */
        public static string normalize(string value)
        {
            string encoded = UrlEncode(value, System.Text.Encoding.GetEncoding("UTF-8"), true);
            // 确保空格编码为%20，符合v3协议要求
            return encoded.Replace("+", "%20");
        }


        /// <summary>
        /// UrlEncode重写：小写转大写，特殊字符特换
        /// </summary>
        /// <param name="strSrc">原字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="bToUpper">是否转大写</param>
        /// <returns></returns>
        public static string UrlEncode(string strSrc, System.Text.Encoding encoding, bool bToUpper)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < strSrc.Length; i++)
            {
                string t = strSrc[i].ToString();
                //string k = HttpUtility.UrlEncode(t, encoding);
                string k = Uri.EscapeDataString(t);
                if (t == k)
                {
                    stringBuilder.Append(t);
                }
                else
                {
                    if (bToUpper)
                        stringBuilder.Append(k.ToUpper());
                    else
                        stringBuilder.Append(k);
                }
            }
            if (bToUpper)
                return stringBuilder.ToString().Replace("+", "%2B").Replace("(", "%28").Replace(")", "%29");
            else
                return stringBuilder.ToString().Replace("+", "%20").Replace("(", "%28").Replace(")", "%29");
        }


    }
}

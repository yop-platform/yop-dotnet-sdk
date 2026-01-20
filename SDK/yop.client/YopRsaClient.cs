using System;
using System.Collections.Generic;
using System.Text;
using SDK.yop.utils;
using SDK.enums;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using Newtonsoft.Json.Linq;
using SDK.common;
using System.Globalization;
using System.Collections; //使用Hashtable时，必须引入这个命名空间
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Net.Http;

namespace SDK.yop.client
{
    public class YopRsaClient
    {
        protected static Dictionary<string, List<string>> uriTemplateCache = new Dictionary<string, List<string>>();

        /// <summary>
        /// 自动补全请求
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodOrUri"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected static string richRequest(string methodOrUri, YopRequest request)
        {
            Assert.notNull(methodOrUri, "method name or rest uri");
            string serverRoot = request.getServerRoot();

            String serverUrl;
            if (methodOrUri.StartsWith("/rest/"))
            {
                if (StringUtils.isBlank(serverRoot))
                {
                    serverRoot = YopConfig.getServerRoot();
                }
                if (methodOrUri.StartsWith(serverRoot))
                {
                    methodOrUri = methodOrUri.Substring(serverRoot.Length + 1);
                }
                request.setServerRoot(serverRoot);
                methodOrUri = mergeTplUri(methodOrUri, request);
                serverUrl = serverRoot + methodOrUri;
                string version = Regex.Match(methodOrUri, "(?<=/rest/v).*?(?=/)").Value;
            }
            else if (methodOrUri.StartsWith("/yos/"))
            {
                if (StringUtils.isBlank(serverRoot))
                {
                    serverRoot = YopConfig.getYosServerRoot();
                }
                if (methodOrUri.StartsWith(serverRoot))
                {
                    methodOrUri = methodOrUri.Substring(serverRoot.Length + 1);
                }
                request.setServerRoot(serverRoot);
                methodOrUri = mergeTplUri(methodOrUri, request);
                serverUrl = serverRoot + methodOrUri;
                string version = Regex.Match(methodOrUri, "(?<=/yos/v).*?(?=/)").Value;
            }
            else
            {
                if (StringUtils.isBlank(serverRoot))
                {
                    serverRoot = YopConfig.getServerRoot();
                }
                request.setServerRoot(serverRoot);
                serverUrl = serverRoot + "/command?" + YopConstants.METHOD + "=" + methodOrUri;
            }
            request.setMethod(methodOrUri);
            return serverUrl;
        }

        /// <summary>
        /// 模板URL自动补全参数
        /// </summary>
        /// <param name="tplUri"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected static string mergeTplUri(string tplUri, YopRequest request)
        {
            string uri = tplUri;
            if (tplUri.IndexOf("{") < 0)
            {
                return uri;
            }
            List<string> dynaParamNames = uriTemplateCache[tplUri];
            if (dynaParamNames == null)
            {
                dynaParamNames = new List<string>();
                dynaParamNames.Add(RegexUtil.GetResResult("\\{([^\\}]+)\\}", tplUri));
                uriTemplateCache.Add(tplUri, dynaParamNames);
            }

            foreach (string dynaParamName in dynaParamNames)
            {
                string value = request.removeParam(dynaParamName);
                Assert.notNull(value, dynaParamName + " must be specified");
                uri = uri.Replace("{" + dynaParamName + "}", value);
            }

            return uri;
        }

        /// <summary>
        /// 发起post请求，以YopResponse对象返回
        /// </summary>
        /// <param name="apiUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns>响应对象</returns>
        public static YopResponse post(String methodOrUri, YopRequest request)
        {
            using (HttpResponseMessage getResponse = postRsaString(methodOrUri, request))
            {
                string content = getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            YopResponse response = new YopResponse();
            response.result = content;
            handleRsaResult(request, response, getResponse);
            return response;
            }
        }

        public static YopResponse postRsa(String methodOrUri, YopRequest request)
        {
            return post(methodOrUri, request);
        }

        public static YopResponse get(String methodOrUri, YopRequest request)
        {
            using (HttpResponseMessage getResponse = getRsaString(methodOrUri, request))
            {
                string content = getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            YopResponse response = response = new YopResponse();
            response.result = content;
            response = handleRsaResult(request, response, getResponse);
            return response;
            }
        }

        public static YopResponse getRsa(String methodOrUri, YopRequest request)
        {
            return get(methodOrUri, request);
        }

        /// <summary>
        /// 发起post请求，以字符串返回
        /// </summary>
        /// <param name="apiUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns>字符串形式的响应</returns>
        public static HttpResponseMessage postRsaString(String methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(methodOrUri, request);
            request.setAbsoluteURL(serverUrl);
            
            Hashtable headers;
            
            // 只有form类型请求需要进行URL编码处理
            if (!StringUtils.hasText(request.getContent()))
            {
                // 先进行一次URL编码，用于签名计算
                request.encoding("sign");
                headers = SignRsaParameter(methodOrUri, request, "POST");
                
                // 再进行一次URL编码，用于HTTP传输（总共两次）
                request.encoding("");
            }
            else
            {
                // JSON请求不需要URL编码处理
                headers = SignRsaParameter(methodOrUri, request, "POST");
            }
            
            return HttpUtils.Send(request, "POST", headers);
        }

        public static HttpResponseMessage getRsaString(String methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(methodOrUri, request);
            request.setAbsoluteURL(serverUrl);

            // GET请求需要URL编码处理
            // 先进行一次URL编码，用于签名计算
            request.encoding("sign");
            Hashtable headers = SignRsaParameter(methodOrUri, request, "GET");
            
            // 再进行一次URL编码，用于HTTP传输（总共两次）
            request.encoding("");
            return HttpUtils.Send(request, "GET", headers);
        }

        /// <summary>
        /// 计算请求内容的SHA256值
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="method">HTTP方法</param>
        /// <returns>SHA256值的十六进制字符串</returns>
        private static string calculateContentSha256(YopRequest request, string method)
        {
            string content = "";
            
            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                if (StringUtils.hasText(request.getContent()))
                {
                    // 如果有直接设置的内容（JSON类型），直接使用原内容
                    content = request.getContent();
                }
                else
                {
                    // 构建请求参数字符串（表单类型）
                    NameValueCollection paramMap = request.getParams();
                    List<string> paramPairs = new List<string>();
                    
                    foreach (string key in paramMap.AllKeys)
                    {
                        string[] values = paramMap.GetValues(key);
                        if (values != null)
                        {
                            foreach (string value in values)
                            {
                                if (StringUtils.isNotBlank(value))
                                {
                                    // 表单类型请求需要URL编码
                                    string encodedKey = Uri.EscapeDataString(key).Replace("+", "%20");
                                    string encodedValue = Uri.EscapeDataString(value).Replace("+", "%20");
                                    paramPairs.Add($"{encodedKey}={encodedValue}");
                                }
                            }
                        }
                    }
                    
                    // 按ASCII顺序排序
                    paramPairs.Sort();
                    content = string.Join("&", paramPairs);
                }
            }
            
            // 计算SHA256，统一使用UTF-8编码
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private static Hashtable SignRsaParameter(String methodOrUri, YopRequest request, String method)
        {
            Assert.notNull(request.getSecretKey(), "secretKey must be specified");
            string appKey = request.getParamValue(YopConstants.APP_KEY);
            if (StringUtils.isBlank(appKey))
            {
                appKey = StringUtils.trimToNull(request
                        .getParamValue(YopConstants.CUSTOMER_NO));
            }

            Assert.notNull(request.getSecretKey(), "secretKey must be specified");

            string timestamp = DateUtils.FormatAlternateIso8601Date(DateTime.Now);
            String requestId = Guid.NewGuid().ToString();
            request.setYopRequestId(requestId);

            Hashtable headers = new Hashtable();
            headers.Add("x-yop-request-id", requestId);
            // SDK 固定标识头（用于服务端识别 SDK 来源与版本）
            headers.Add("x-yop-sdk-langs", ".net");
            headers.Add("x-yop-sdk-version", YopConstants.CLIENT_VERSION);

            // 计算内容SHA256
            string contentSha256 = calculateContentSha256(request, method);
            headers.Add("x-yop-content-sha256", contentSha256);

            string protocolVersion = "yop-auth-v3";
            string EXPIRED_SECONDS = "1800";

            string authString = protocolVersion + "/" + appKey + "/" + timestamp + "/" + EXPIRED_SECONDS;

            List<string> headersToSignSet = new List<string>();
            headersToSignSet.Add("x-yop-request-id");
            headersToSignSet.Add("x-yop-content-sha256");

            if (StringUtils.isBlank(request.getCustomerNo()))
            {
                headers.Add("x-yop-appkey", appKey);
                headersToSignSet.Add("x-yop-appkey");
            }
            else
            {
                headers.Add("x-yop-customerid", appKey);
                headersToSignSet.Add("x-yop-customerid");
            }

            // Formatting the URL with signing protocol.
            string canonicalURI = HttpUtils.getCanonicalURIPath(methodOrUri);

            // Formatting the query string with signing protocol.
            // v3协议要求：
            // - GET：canonicalQueryString 为 URL 查询串（SDK这里由 paramMap 生成）
            // - POST + form：canonicalQueryString 恒为空字符串
            // - POST + json：canonicalQueryString 仅取 URL 查询串（通常为空）；绝不能把 body/paramMap 拼进来
            //
            // 当前 SDK 的 POST + json 请求不会把 paramMap 附加到 URL 上（HttpUtils 会直接发送 JSON body），
            // 如果把 paramMap 当作 canonicalQueryString，会造成服务端验签失败。
            string canonicalQueryString = "";
            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                canonicalQueryString = getCanonicalQueryString(request, true);
            }

            //Sorted the headers should be signed from the request.
            SortedDictionary<String, String> headersToSign = getHeadersToSign(headers, headersToSignSet);

            // Formatting the headers from the request based on signing protocol.
            string canonicalHeader = getCanonicalHeaders(headersToSign);

            string signedHeaders = "";
            if (headersToSign != null)
            {
                List<string> sortedHeaderNames = new List<string>();
                foreach (string key in headersToSign.Keys)
                {
                    sortedHeaderNames.Add(key.ToLower());
                }
                sortedHeaderNames.Sort();
                
                for (int i = 0; i < sortedHeaderNames.Count; i++)
                {
                    if (i > 0)
                    {
                        signedHeaders += ";";
                    }
                    signedHeaders += sortedHeaderNames[i];
                }
            }

            string canonicalRequest = authString + "\n" + method + "\n" + canonicalURI + "\n" + canonicalQueryString + "\n" + canonicalHeader;
            string private_key = request.getSecretKey();
            string signToBase64 = SHA1withRSA.sign(canonicalRequest, private_key, "UTF-8");
            signToBase64 = signToBase64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            signToBase64 += "$SHA256";
            headers.Add("Authorization", "YOP-RSA2048-SHA256 " + protocolVersion + "/" + appKey + "/" + timestamp + "/" + EXPIRED_SECONDS + "/" + signedHeaders + "/" + signToBase64);
            return headers;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="apiUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns>响应对象</returns>
        public static YopResponse upload(String apiUri, YopRequest request)
        {
            using (HttpResponseMessage httpWebResponse = uploadRsaForString(apiUri, request))
            {
                string content = httpWebResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            YopResponse response = new YopResponse();
            response.result = content;
            
            handleRsaResult(request, response, httpWebResponse);
            return response;
            }
        }

        public static YopResponse uploadRsa(String apiUri, YopRequest request)
        {
            return upload(apiUri, request);
        }
        
        /// <summary>
        /// 发起文件上传请求，以字符串返回
        /// </summary>
        /// <param name="apiUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns>字符串形式的响应</returns>
        public static HttpResponseMessage uploadRsaForString(String apiUri, YopRequest request)
        {
            string serverUrl = richRequest(apiUri, request);
            request.setAbsoluteURL(serverUrl);
            //request.encoding("");
            string strTemp = request.getParamValue("_file");

            request.removeParam("_file");

            Hashtable headers = SignRsaParameter(apiUri, request, "POST");

            request.addParam("_file", strTemp);

            return HttpUtils.Send(request, "PUT", headers);
        }

        protected static YopResponse handleRsaResult(YopRequest request, YopResponse response, HttpResponseMessage httpWebResponse)
        {
            string sign = null;
            if (httpWebResponse.Headers.TryGetValues("x-yop-sign", out IEnumerable<string> values))
            {
                sign = values.FirstOrDefault();
            }
            if (string.IsNullOrEmpty(sign))
                return response;
            response.validSign = isValidResult(response.result.ToString(),sign, request.getYopPublicKey());
            return response;
        }

        /// <summary>
        /// 对业务结果签名进行校验
        /// </summary>
        /// <param name="result"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        /// 
        public static bool isValidResult(String result, String sign, String publicKey)
        {
            if (string.IsNullOrWhiteSpace(sign))
            {
                return false;
            }

            // v3 的签名值通常为 Base64URL-safe 且可能带 "$SHA256" 后缀（与请求 Authorization 的 signature 一致风格）
            // 而 SHA1withRSA.verify 内部使用 Convert.FromBase64String()，只接受标准 Base64。
            // 这里做一次兼容转换：去掉后缀 + Base64URL -> 标准 Base64。
            string signPart = sign.Trim();
            int dollarIndex = signPart.IndexOf('$');
            if (dollarIndex > 0)
            {
                signPart = signPart.Substring(0, dollarIndex);
            }

            string standardBase64Sign;
            try
            {
                // 兼容 Base64URL（-/_ 且可能缺少 padding）
                byte[] signBytes = Base64SecureURL.Decode(signPart);
                standardBase64Sign = Convert.ToBase64String(signBytes);
            }
            catch
            {
                // 兜底：如果本身就是标准 Base64，则直接使用
                standardBase64Sign = signPart;
            }

            // 按 v3 协议参考 Java 实现：
            // content = content.replaceAll("[ \t\n]", "")
            // 注意：这里仅移除 space / tab / '\n'；不做 JSON 语义级重排。
            string plainText = result ?? string.Empty;
            plainText = plainText.Replace(" ", string.Empty)
                                 .Replace("\t", string.Empty)
                                 .Replace("\n", string.Empty);

            return SHA1withRSA.verify(plainText, standardBase64Sign, publicKey, "UTF-8");
        }

        private static SortedDictionary<String, String> getHeadersToSign(Hashtable headers, List<string> headersToSign)
        {
            SortedDictionary<String, String> ret = new SortedDictionary<string, string>();

            if (headersToSign != null)
            {
                List<string> tempSet = new List<string>();

                foreach (Object header in headersToSign)
                {
                    tempSet.Add((string)header.ToString().ToLower());
                }

                headersToSign = tempSet;
            }

            foreach (DictionaryEntry de in headers)
            {
                if (de.Value != null)
                {
                    if ((headersToSign == null && isDefaultHeaderToSign((string)de.Key)) || (headersToSign != null && headersToSign.Contains((String)de.Key.ToString().ToLower()) && (String)de.Key.ToString().ToLower() != "Authorization"))
                    {
                        ret.Add((string)de.Key, (string)de.Value);
                    }
                }
                Console.WriteLine("Key -- {0}; Value --{1}.", de.Key, de.Value);
            }

            return ret;
        }

        private static bool isDefaultHeaderToSign(string header)
        {
            header = header.Trim().ToLower();
            List<string> defaultHeadersToSign = new List<string>();
            defaultHeadersToSign.Add("host");
            defaultHeadersToSign.Add("content-length");
            defaultHeadersToSign.Add("content-type");

            return header.StartsWith("x-yop-") || defaultHeadersToSign.Contains(header);
        }

        /**
 * @param $headers
 * @return string
 */
        public static string getCanonicalHeaders(SortedDictionary<String, String> headers)
        {
            if (headers == null)
            {
                return "";
            }

            List<string> headerStrings = new List<string>();

            foreach (string key in headers.Keys)
            {
                string value = (string)headers[key];
                if (key == null)
                {
                    continue;
                }
                if (value == null)
                {
                    value = "";
                }

                string kv = HttpUtils.normalize(key.Trim().ToLower());
                value = HttpUtils.normalize(value.Trim());
                headerStrings.Add(kv + ':' + value);
            }

            headerStrings.Sort((a, b) => string.CompareOrdinal(a, b));  //20190114 改为Java排序规则

            string StrQuery = "";

            foreach (Object kv in headerStrings)
            {
                if (StrQuery == "")
                {
                    StrQuery += "";
                }
                else
                {
                    StrQuery += "\n";
                }

                StrQuery += (string)kv;
            }

            return StrQuery;
        }

        /// <summary>
        /// 从完整返回结果中获取业务结果，主要用于验证返回结果签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static string getBizResult(string content, FormatType format)
        {
            if (StringUtils.isBlank(content))
            {
                return content;
            }

            switch (format)
            {
                case FormatType.json:
                    //旧版返参处理
                    string jsonStr = StringUtils.substringAfter(content, "\"result\" : ");
                    jsonStr = StringUtils.substringBeforeLast(jsonStr, "\"ts\"");
                    jsonStr = StringUtils.substringBeforeLast(jsonStr, ",");// 去除逗号

                    /*
                    //新版返参处理
                    string jsonStr = StringUtils.substringAfter(content, "\"result\" : ");
                    jsonStr = StringUtils.substringBeforeLast(jsonStr, "}");// 去除最后大括号
                    */
                    //return jsonStr.Replace("\"", "");
                    return jsonStr;
                default:
                    string xmlStr = StringUtils.substringAfter(content, "</state>");
                    xmlStr = StringUtils.substringBeforeLast(xmlStr, "<ts>");
                    return xmlStr;
            }
        }

        /**
        * @param $YopRequest
        * @param $forSignature
        * @return string
        */
        public static string getCanonicalQueryString(YopRequest request, bool forSignature)
        {
            List<string> arrayList = new List<string>();

            string StrQuery = "";
            NameValueCollection paramMap = request.getParams();

            string[] values = null;

            foreach (string key in paramMap.Keys)
            {
                values = paramMap.GetValues(key);
                foreach (string value in values)
                {
                    if (forSignature && key.CompareTo("Authorization") == 0)
                    {
                        continue;
                    }
                    
                    // 只有form类型请求需要URL编码，JSON请求不需要
                    if (StringUtils.hasText(request.getContent()))
                    {
                        // JSON请求，参数不需要URL编码
                        arrayList.Add(key + "=" + value);
                    }
                    else
                    {
                        // 表单请求，参数需要URL编码，确保空格编码为%20
                        string encodedKey = Uri.EscapeDataString(key).Replace("+", "%20");
                        string encodedValue = Uri.EscapeDataString(value).Replace("+", "%20");
                        arrayList.Add(encodedKey + "=" + encodedValue);
                    }
                }
            }
			
            arrayList.Sort((a, b) => string.CompareOrdinal(a, b));  //20181102 改为Java排序规则

            for (int i = 0; i < arrayList.Count; i++)
            {
                if (StrQuery == "")
                {
                    StrQuery += "";
                }
                else
                {
                    StrQuery += "&";
                }
                StrQuery += (string)arrayList[i];
            }

            return StrQuery;
        }
    }
}

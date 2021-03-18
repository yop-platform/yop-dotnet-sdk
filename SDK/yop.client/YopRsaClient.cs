﻿using System;
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
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
            if (methodOrUri.StartsWith(request.getServerRoot()))
            {
                methodOrUri = methodOrUri.Substring(request.getServerRoot().Length + 1);
            }

            String serverUrl;
            if (methodOrUri.StartsWith("/rest/"))
            {
                methodOrUri = mergeTplUri(methodOrUri, request);
                serverUrl = request.getServerRoot() + methodOrUri;
                string version = Regex.Match(methodOrUri, "(?<=/rest/v).*?(?=/)").Value;
                if (StringUtils.isNotBlank(version))
                {
                    request.setVersion(version);
                }
            }
            else if (methodOrUri.StartsWith("/yos/"))
            {
                request.setServerRoot(YopConfig.getYosServerRoot());
                methodOrUri = mergeTplUri(methodOrUri, request);
                serverUrl = request.getServerRoot() + methodOrUri;
                string version = Regex.Match(methodOrUri, "(?<=/yos/v).*?(?=/)").Value;
                if (StringUtils.isNotBlank(version))
                {
                    request.setVersion(version);
                }
            }
            else
            {
                serverUrl = request.getServerRoot() + "/command?" + YopConstants.METHOD + "=" + methodOrUri;
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
        public static YopResponse postRsa(String methodOrUri, YopRequest request)
        {
            HttpWebResponse getResponse = postRsaString(methodOrUri, request);
            Stream stream = getResponse.GetResponseStream();
            string content = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
            YopResponse response = new YopResponse();
            response.result = content;
            handleRsaResult(request, response, getResponse);
            return response;
        }

        public static YopResponse getRsa(String methodOrUri, YopRequest request)
        {
            HttpWebResponse getResponse = getRsaString(methodOrUri, request);
            Stream stream = getResponse.GetResponseStream();
            string content = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

            YopResponse response = response = new YopResponse();
            response.result = content;
            response = handleRsaResult(request, response, getResponse);
            return response;
        }

        /// <summary>
        /// 发起post请求，以字符串返回
        /// </summary>
        /// <param name="apiUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns>字符串形式的响应</returns>
        public static HttpWebResponse postRsaString(String methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(methodOrUri, request);
            request.setAbsoluteURL(serverUrl);
            Hashtable headers = SignRsaParameter(methodOrUri, request, "POST");

            request.encoding("");
            HttpWebResponse getResponse = HttpUtils.PostAndGetHttpWebResponse(request, "POST", headers);
            return getResponse;
        }

        public static HttpWebResponse getRsaString(String methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(methodOrUri, request);
            request.setAbsoluteURL(serverUrl);

            Hashtable headers = SignRsaParameter(methodOrUri, request, "GET");
            request.encoding("");
            HttpWebResponse getResponse = HttpUtils.PostAndGetHttpWebResponse(request, "GET", headers);
            return getResponse;
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
            headers.Add("x-yop-date", timestamp);
            headers.Add("x-yop-sdk-version", YopConfig.getSdkVersion());
            headers.Add("x-yop-sdk-langs", YopConfig.getSdkLangs());

            string protocolVersion = "yop-auth-v2";
            string EXPIRED_SECONDS = "1800";

            string authString = protocolVersion + "/" + appKey + "/" + timestamp + "/" + EXPIRED_SECONDS;

            List<string> headersToSignSet = new List<string>();
            headersToSignSet.Add("x-yop-request-id");
            headersToSignSet.Add("x-yop-date");

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
            string canonicalQueryString = getCanonicalQueryString(request, true);

            //Sorted the headers should be signed from the request.
            SortedDictionary<String, String> headersToSign = getHeadersToSign(headers, headersToSignSet);

            // Formatting the headers from the request based on signing protocol.
            string canonicalHeader = getCanonicalHeaders(headersToSign);

            string signedHeaders = "";
            if (headersToSignSet != null)
            {
                foreach (string key in headersToSign.Keys)
                {
                    string value = (string)headersToSign[key];
                    if (signedHeaders == "")
                    {
                        signedHeaders += "";
                    }
                    else
                    {
                        signedHeaders += ";";
                    }
                    signedHeaders += key;
                }
                signedHeaders = signedHeaders.ToLower();
            }

            string canonicalRequest = authString + "\n" + method + "\n" + canonicalURI + "\n" + canonicalQueryString + "\n" + canonicalHeader;
            string private_key = request.getSecretKey();
            string signToBase64 = SHA1withRSA.sign(canonicalRequest, private_key, "UTF-8");
            signToBase64 = Base64SecureURL.Encode(signToBase64);
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
        public static YopResponse uploadRsa(String apiUri, YopRequest request)
        {
            HttpWebResponse httpWebResponse = uploadRsaForString(apiUri, request);
            Stream stream = httpWebResponse.GetResponseStream();
            string content = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

            YopResponse response = new YopResponse();
            response.result = content;
            
            handleRsaResult(request, response, httpWebResponse);
            return response;
        }
        /// <summary>
        /// 发起文件上传请求，以字符串返回
        /// </summary>
        /// <param name="apiUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns>字符串形式的响应</returns>
        public static HttpWebResponse uploadRsaForString(String apiUri, YopRequest request)
        {
            string serverUrl = richRequest(apiUri, request);
            request.setAbsoluteURL(serverUrl);
            //request.encoding("");
            string strTemp = request.getParamValue("_file");

            request.removeParam("_file");

            Hashtable headers = SignRsaParameter(apiUri, request, "POST");

            request.addParam("_file", strTemp);

            HttpWebResponse httpWebResponse = HttpUtils.PostAndGetHttpWebResponse(request, "PUT", headers);
            return httpWebResponse;
        }

        protected static YopResponse handleRsaResult(YopRequest request, YopResponse response, HttpWebResponse httpWebResponse)
        {
            string sign = httpWebResponse.Headers["x-yop-sign"];
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
            string sb = "";
            if (result == null)
            {
                sb = "";
            }
            else
            {
                sb = result.Trim();
            }

            sb = sb.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            return SHA1withRSA.verify(sb, sign, publicKey, "UTF-8");
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
                    arrayList.Add(key + "=" + HttpUtils.normalize(value));
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

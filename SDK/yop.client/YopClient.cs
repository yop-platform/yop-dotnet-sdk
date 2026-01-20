using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SDK.yop.utils;
using SDK.enums;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using Newtonsoft.Json.Linq;


namespace SDK.yop.client
{
    using encrypt;
    using common;
    public class YopClient
    {
        protected static Dictionary<string, List<string>> uriTemplateCache = new Dictionary<string, List<string>>();
        /// <summary>
        /// 发起post请求，以YopResponse对象返回
        /// </summary>
        /// <param name="methodOrUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns></returns>
        public static YopResponse post(string methodOrUri, YopRequest request)
        {
            string content = postForString(methodOrUri, request);
            System.Diagnostics.Debug.WriteLine("请求结果：" + content);
            //格式化结果
            //YopResponse response = YopMarshallerUtils.unmarshal(content,
            //        request.getFormat(), YopResponse.class);
            //  return response;
            YopResponse response = new YopResponse();
            response.result = content;
            return response;
        }

        /// <summary>
        /// 发起get请求，以YopResponse对象返回
        /// </summary>
        /// <param name="methodOrUri"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static YopResponse get(string methodOrUri, YopRequest request)
        {
            string content = getForString(methodOrUri, request);
            //YopResponse response = YopMarshallerUtils.unmarshal(content,
            //        request.getFormat(), YopResponse.class);
            //  handleResult(request, response, content);
            YopResponse response = new YopResponse();
            response.result = content;
            return response;
        }

        public static YopResponse upload(String methodOrUri, YopRequest request)
        {
            string content = uploadForString(methodOrUri, request);
            //YopResponse response = YopMarshallerUtils.unmarshal(content,
            //        request.getFormat(), YopResponse.class);
            YopResponse response = null;
            if (request.getFormat() == FormatType.json)
            {
                response = (YopResponse)JsonConvert.DeserializeObject(content, typeof(YopResponse));
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                string jsonText = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                JObject jo = JObject.Parse(jsonText);
                string strValue = jo["response"].ToString();
                response = (YopResponse)JsonConvert.DeserializeObject(strValue, typeof(YopResponse));
            }

            handleResult(request, response, content);
            return response;
        }

        protected static void handleResult(YopRequest request, YopResponse response, string content)
        {
            response.format = request.getFormat();
            string ziped = string.Empty;
            if (response.isSuccess())
            {
                string strResult = getBizResult(content, request.getFormat());
                ziped = strResult.Replace("\t\n", "");
                // 先解密，极端情况可能业务正常，但返回前处理（如加密）出错，所以要判断是否有error
                if (StringUtils.isNotBlank(strResult) && response.error == null)
                {
                    if (request.isEncrypt())
                    {
                        string decryptResult = decrypt(request, strResult.Trim());
                        response.stringResult = decryptResult;
                        response.result = decryptResult;
                        ziped = decryptResult.Replace("\t\n", "");
                    }
                    else
                    {
                        response.stringResult = strResult;
                    }
                }
            }

            // 再验签
            if (request.isSignRet() && StringUtils.isNotBlank(response.sign))
            {
                string signStr = response.state + ziped + response.ts;
                response.validSign = YopSignUtils.isValidResult(signStr,
                        request.getSecretKey(), request.getSignAlg(),
                        response.sign);
            }
            else
            {
                response.validSign = true;
            }
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
                    string jsonStr = StringUtils.substringAfter(content, "\"result\" : ");
                    jsonStr = StringUtils.substringBeforeLast(jsonStr, "\"ts\"");
                    // 去除逗号
                    jsonStr = StringUtils.substringBeforeLast(jsonStr, ",");
                    return jsonStr.Replace("\"", "");
                default:
                    string xmlStr = StringUtils.substringAfter(content, "</state>");
                    xmlStr = StringUtils.substringBeforeLast(xmlStr, "<ts>");
                    return xmlStr;
            }
        }

        /// <summary>
        /// 发起get请求，以字符串返回
        /// </summary>
        /// <param name="methodOrUri"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string getForString(string methodOrUri, YopRequest request)
        {
            //String serverUrl = buildURL(methodOrUri, request);
            //request.setAbsoluteURL(serverUrl);
            //String content = getRestTemplate(request).getForObject(serverUrl, String.class);
            //  if (logger.isDebugEnabled()) {
            //      logger.debug("response:\n" + content);
            //  }
            string serverUrl = richRequest(HttpMethodType.GET, methodOrUri,
                     request);
            Hashtable headers = signAndEncrypt(request);
            request.setAbsoluteURL(serverUrl);
            request.encoding("");
            //请求网站

            using (var response = HttpUtils.Send(request, "GET", headers))
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 发起post请求，以字符串返回
        /// </summary>
        /// <param name="methodOrUri">目标地址或命名模式的method</param>
        /// <param name="request">客户端请求对象</param>
        /// <returns></returns>
        public static string postForString(string methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(HttpMethodType.POST, methodOrUri,
                    request);
            Hashtable headers = signAndEncrypt(request);
            request.setAbsoluteURL(serverUrl);
            request.encoding("");
            //请求网站

            using (var response = HttpUtils.Send(request, "POST", headers))
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        public static string uploadForString(string methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(HttpMethodType.POST, methodOrUri,
                    request);


            string strTemp = request.getParamValue("_file");
            request.removeParam("_file");

            Hashtable headers = signAndEncrypt(request);

            request.addParam("_file", strTemp);

            request.setAbsoluteURL(serverUrl);
            request.encoding("blowfish");
            //请求网站

            using (var response = HttpUtils.Send(request, "PUT", headers))
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 简单校验及请求签名
        /// </summary>
        /// <param name="request"></param>
        public static Hashtable signAndEncrypt(YopRequest request)
        {
            Assert.notNull(request.getMethod(), "method must be specified");
            Assert.notNull(request.getSecretKey(), "secretKey must be specified");
            string appKey = request.getParamValue(YopConstants.APP_KEY);
            if (StringUtils.isBlank(appKey))
            {
                appKey = StringUtils.trimToNull(request
                        .getParamValue(YopConstants.CUSTOMER_NO));
            }
            Assert.notNull(appKey, "appKey 与 customerNo 不能同时为空");
            string signValue = YopSignUtils.sign(toSimpleMap(request.getParams()),
                    request.getIgnoreSignParams(), request.getSecretKey(),
                    request.getSignAlg());
            Hashtable headers = new Hashtable();

            string timestamp = DateUtils.FormatAlternateIso8601Date(DateTime.Now);

            //request.addParam(YopConstants.SIGN, signValue);
            headers.Add("x-yop-appkey", appKey);
            headers.Add("x-yop-date", timestamp);
            headers.Add("Authorization", "YOP-HMAC-AES128 " + signValue);

            String requestId = Guid.NewGuid().ToString("N");
            request.setYopRequestId(requestId);

            headers.Add("x-yop-sdk-langs", YopConfig.getSdkLangs());
            headers.Add("x-yop-sdk-version", YopConfig.getSdkVersion());
            headers.Add("x-yop-request-id", requestId);

            if (request.IsRest())
            {
                request.removeParam(YopConstants.METHOD);
                request.removeParam(YopConstants.VERSION);
            }

            // 签名之后再加密
            if (request.isEncrypt())
            {
                try
                {
                    encrypt(request);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            return headers;
        }


        protected internal static IDictionary<string, string> toSimpleMap(NameValueCollection form)
        {
            IDictionary<string, string> map = new Dictionary<string, string>();
            foreach (string key in form.AllKeys)
            {
                map.Add(key, form[key]);
            }
            return map;
        }


        /**
         * 请求加密，使用AES算法，要求secret为正常的AESkey
         *
         * @throws Exception
         */
        protected static void encrypt(YopRequest request)
        {
            StringBuilder builder = new StringBuilder();
            bool first = true;
            NameValueCollection myparams = request.getParams();
            foreach (string key in myparams.AllKeys)
            {
                if (YopConstants.isProtectedKey(key))
                {
                    continue;
                }

                string[] strValues = myparams.GetValues(key);
                List<string> values = new List<string>();
                foreach (string s in strValues)
                {
                    values.Add(s);
                }
                myparams.Remove(key);
                if (values == null || values.Count == 0)
                {
                    continue;
                }
                foreach (string v in values)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append("&");
                    }
                    // 避免解密后解析异常，此处需进行encode（此逻辑在整个request做encoding前）
                    builder.Append(key).Append("=").Append(HttpUtility.UrlEncode(v, Encoding.UTF8));//YopConstants.ENCODING
                }
            }
            string encryptBody = builder.ToString();
            if (StringUtils.isBlank(encryptBody))
            {
                // 没有需加密的参数，则只标识响应需加密
                request.addParam(YopConstants.ENCRYPT, true);
            }
            else
            {
                if (StringUtils.isNotBlank(request
                        .getParamValue(YopConstants.APP_KEY)))
                {
                    // 开放应用使用AES加密
                    string encrypt = AESEncrypter.encrypt(encryptBody,
                            request.getSecretKey());
                    request.addParam(YopConstants.ENCRYPT, encrypt);
                }
                else
                {
                    // 商户身份调用使用Blowfish加密
                    string encrypt = BlowFish.Encrypt(encryptBody,
                            request.getSecretKey());

                    request.addParam(YopConstants.ENCRYPT, encrypt);
                }
            }
        }

        protected static string decrypt(YopRequest request, string strResult)
        {
            if (request.isEncrypt() && StringUtils.isNotBlank(strResult))
            {
                if (StringUtils.isNotBlank(request.getParamValue(YopConstants.APP_KEY)))
                {
                    strResult = AESEncrypter.decrypt(strResult,
                            request.getSecretKey());
                }
                else
                {
                    strResult = BlowFish.Decrypt(strResult,
                            request.getSecretKey());
                }
            }
            return strResult;
        }
        /// <summary>
        /// 自动补全请求
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodOrUri"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected static string richRequest(HttpMethodType type, string methodOrUri, YopRequest request)
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
                request.setRest(true);
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
                request.setRest(true);
                request.setServerRoot(serverRoot);
                methodOrUri = mergeTplUri(methodOrUri, request);
                serverUrl = serverRoot + methodOrUri;
                string version = Regex.Match(methodOrUri, "(?<=/yos/v).*?(?=/)").Value;
            }
            else
            {
                serverUrl = YopConfig.getServerRoot() + "/command?" + YopConstants.METHOD + "=" + methodOrUri;
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
    }
}

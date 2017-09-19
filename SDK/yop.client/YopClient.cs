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
            signAndEncrypt(request);
            request.setAbsoluteURL(serverUrl);
            request.encoding("");
            //请求网站

            Stream stream = HttpUtils.PostAndGetHttpWebResponse(request, "GET").GetResponseStream();
            string content = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
            return content;
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
            signAndEncrypt(request);
            request.setAbsoluteURL(serverUrl);
            request.encoding("");
            //请求网站

            Stream stream = HttpUtils.PostAndGetHttpWebResponse(request, "POST").GetResponseStream();
            string content = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
            return content;
            //string content = getRestTemplate(request).postForObject(serverUrl,
            //        request.getParams(), string.class);
            //  if (logger.isDebugEnabled()) {
            //      logger.debug("response:\n" + content);
            //  }
            //  return content;
            //return null;
        }

        public static string uploadForString(string methodOrUri, YopRequest request)
        {
            string serverUrl = richRequest(HttpMethodType.POST, methodOrUri, request);

            //NameValueCollection original = request.getParams();
            //NameValueCollection alternate = new NameValueCollection();
            List<string> uploadFiles = request.getParam("fileURI");
            if (null == uploadFiles || uploadFiles.Count == 0)
            {
                throw new SystemException("上传文件时参数_file不能为空!");
            }

            List<UploadFile> upfiles = new List<UploadFile>();
            foreach (string strPath in uploadFiles)
            {
                UploadFile files = new UploadFile();
                files.Name = "_file";
                files.Filename = Path.GetFileName(strPath);

                Stream oStream = File.OpenRead(strPath.Split(':')[1]);
                byte[] arrBytes = new byte[oStream.Length];
                int offset = 0;
                while (offset < arrBytes.LongLength)
                {
                    offset += oStream.Read(arrBytes, offset, arrBytes.Length - offset);
                }
                files.Data = arrBytes;
                upfiles.Add(files);
            }

            signAndEncrypt(request);
            request.setAbsoluteURL(serverUrl);
            request.encoding("blowfish");

            Stream stream = HttpUtils.PostFile(request, upfiles).GetResponseStream();
            string content = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
            return content;


            //foreach(string uploadFile in uploadFiles)
            //{
            //  try
            //  {
            //    alternate.Add("fileURI", new UrlResource(new URI(uploadFile)));

            //  }
            //  catch (Exception e)
            //  {
            //    System.Diagnostics.Debug.WriteLine("_file upload error.", e);
            //  }
            //}

            //signAndEncrypt(request);
            //request.setAbsoluteURL(serverUrl);
            //request.encoding();

            //foreach (string key in original.AllKeys)
            //{
            //  //alternate.put(key, new ArrayList<Object>(original.get(key)));
            //  alternate.Add(key, original.Get(key));
            //}

            ////string content = getRestTemplate(request).postForObject(serverUrl, alternate, String.class);
            ////  //if (logger.isDebugEnabled()) {
            ////  //    logger.debug("response:\n" + content);
            ////  //}
            ////  return content;
            //return null;
        }

        //private static RestTemplate getRestTemplate(YopRequest request)
        //{
        //  if (null != request.ConnectTimeout || null != request.ReadTimeout)
        //  {
        //    int connectTimeout = null != request.ConnectTimeout ? request.ConnectTimeout.intValue() : YopConfig.ConnectTimeout;
        //    int readTimeout = null != request.ReadTimeout ? request.ReadTimeout.intValue() : YopConfig.ReadTimeout;
        //    return new YopRestTemplate(connectTimeout, readTimeout);
        //  }
        //  else
        //  {
        //    return restTemplate;
        //  }
        //}


        /// <summary>
        /// 简单校验及请求签名
        /// </summary>
        /// <param name="request"></param>
        public static void signAndEncrypt(YopRequest request)
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
            request.addParam(YopConstants.SIGN, signValue);
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
            if (methodOrUri.StartsWith(request.getServerRoot()))//检查是否以指定字符开头
            {
                methodOrUri = methodOrUri.Substring(request.getServerRoot().Length);
            }
            bool isRest = methodOrUri.StartsWith("/rest/");//检查是否以指定字符开头
            request.setRest(isRest);//临时变量，避免多次判断
            string serverUrl = request.getServerRoot();
            if (isRest)
            {
                methodOrUri = mergeTplUri(methodOrUri, request);
                serverUrl += methodOrUri;
                string version = Regex.Match(methodOrUri, "(?<=/rest/v).*?(?=/)").Value;
                if (StringUtils.isNotBlank(version))
                {
                    request.setVersion(version);
                }
            }
            else
            {
                serverUrl += "/command?" + YopConstants.METHOD + "=" + methodOrUri;
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

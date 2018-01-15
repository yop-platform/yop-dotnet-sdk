using System;
using System.Collections.Generic;
using System.Text;
using SDK.yop.utils;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Web;


namespace SDK.yop.client
{
    using common;
    using SDK.enums;

    public class YopRequest
    {
        private FormatType format = FormatType.json;

        private string method;

        private string locale = "zh_CN";

        private string version = "1.0";

        private string signAlg = YopConstants.ALG_SHA1;

        /// <summary>
        /// 商户编号，易宝商户可不注册开放应用(获取appKey)也可直接调用API
        /// </summary>
        private string customerNo;

        //private Dictionary<string, string> paramMap = new Dictionary<string, string>();
        private NameValueCollection paramMap = new NameValueCollection();

        private List<string> ignoreSignParams = new List<string>() { YopConstants.SIGN };//Arrays.asList(YopConstants.SIGN);

        /// <summary>
        /// 报文是否加密，如果请求加密，则响应也加密，需做解密处理
        /// </summary>
        private bool encrypt = false;

        /// <summary>
        /// 业务结果是否签名，默认不签名
        /// </summary>
        private bool signRet = false;

        /// <summary>
        /// 连接超时时间
        /// </summary>
        private int connectTimeout = 30000;

        /// <summary>
        /// 读取返回结果超时
        /// </summary>
        private int readTimeout = 60000;

        /// <summary>
        /// 临时变量，避免多次判断
        /// </summary>
        [NonSerialized]
        private bool isRest = true;

        /// <summary>
        /// 可支持不同请求使用不同的appKey及secretKey
        /// </summary>
        private string appKey;

        /// <summary>
        /// 可支持不同请求使用不同的appKey及secretKey,secretKey只用于本地签名，不会被提交
        /// </summary>
        private string secretKey;

        /// <summary>
        /// 可支持不同请求使用不同的appKey及secretKey,secretKey只用于本地签名，不会被提交
        /// </summary>
        private string yopPublicKey;


        /// <summary>
        /// 可支持不同请求使用不同的appKey及secretKey、serverRoot,secretKey只用于本地签名，不会被提交
        /// </summary>
        private string serverRoot;

        /// <summary>
        /// 临时变量，请求绝对路径
        /// </summary>
        private string absoluteURL;

        public YopRequest()
        {

            this.appKey = YopConfig.getAppKey();
            this.secretKey = YopConfig.getSecret();
            this.serverRoot = YopConfig.getServerRoot();
            if (YopConfig.getAppKey() != null)
                paramMap.Add(YopConstants.APP_KEY, YopConfig.getAppKey());
            //paramMap.Add(YopConstants.FORMAT, format.ToString());
            paramMap.Add(YopConstants.VERSION, version);
            paramMap.Add(YopConstants.LOCALE, locale);
            paramMap.Add(YopConstants.TIMESTAMP, GetTimeStamp(DateTime.Now));
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(System.DateTime time, int length = 13)
        {
            long ts = ConvertDateTimeToInt(time);
            return ts.ToString().Substring(0, length);
        }

        /// <summary>
        /// 同一个工程内部可支持多个开放应用发起调用
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="secretKey"></param>
        public YopRequest(string appKey, string secretKey)
            : this()
        {
            this.appKey = appKey;
            this.secretKey = secretKey;
            if (appKey != null)
                paramMap.Set(YopConstants.APP_KEY, appKey);
        }

        /// <summary>
        /// 同一个工程内部可支持多个开放应用发起调用，且支持调不同的服务器
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="serverRoot"></param>
        public YopRequest(string appKey, string secretKey, string serverRoot, string yopPublicKey)
            : this(appKey, secretKey)
        {
            this.serverRoot = serverRoot;
            this.yopPublicKey = yopPublicKey;
        }

        public YopRequest addParam(string paramName, object paramValue)
        {
            addParam(paramName, paramValue, false);
            return this;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值：如果为集合或数组类型，则自动拆解，最终想作为数组提交到服务端</param>
        /// <param name="ignoreSign">是否忽略签名</param>
        /// <returns></returns>
        public YopRequest addParam(string paramName, object paramValue, bool ignoreSign)
        {
            Assert.hasText(paramName, "参数名不能为空");
            if (paramValue == null ||
              ((paramValue is string) && StringUtils.isBlank((string)paramValue))
                || ((paramValue is IDictionary) && ((IDictionary)paramValue).Count == 0))
            {
                //logger.warn("参数" + paramName + "为空，忽略");
                return this;
            }
            if (YopConstants.isProtectedKey(paramName))
            {
                paramMap.Set(paramName, paramValue.ToString().Trim());
                return this;
            }
            if (paramValue is IDictionary)
            {
                // 集合类
                foreach (object o in (IDictionary)paramValue)
                {
                    if (o != null)
                    {
                        paramMap.Add(paramName, o.ToString().Trim());
                    }
                }
            }
            else if (paramValue is Array)
            {
                // 数组
                int len = (paramValue as Array).Length;
                for (int i = 0; i < len; i++)
                {
                    object o = (paramValue as Array).GetValue(i);// Array.get(paramValue, i);
                    if (o != null)
                    {
                        paramMap.Add(paramName, o.ToString().Trim());
                    }
                }
            }
            else
            {
                paramMap.Add(paramName, paramValue.ToString().Trim());
            }

            if (ignoreSign)
            {
                ignoreSignParams.Add(paramName);
            }
            return this;
        }


        public List<string> getParam(string key)
        {
            List<string> list = new List<string>();
            string[] values = paramMap[key].Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                list.Add(values[i]);
            }
            return list;
            //return paramMap.get(key);
        }

        public string getParamValue(string key)
        {
            //return StringUtils.join(paramMap.get(key), ",");
            return paramMap[key];
        }

        public string removeParam(string key)
        {
            paramMap.Remove(key);
            return paramMap[key];
            //return StringUtils.join(paramMap.remove(key), ",");
        }

        public NameValueCollection getParams()
        {
            return paramMap;
        }

        public List<string> getIgnoreSignParams()
        {
            return ignoreSignParams;
        }

        public void setFormat(FormatType format)
        {
            Assert.notNull(format);
            this.format = format;
            paramMap.Set(YopConstants.FORMAT, this.format.ToString());
        }

        public void setLocale(string locale)
        {
            this.locale = locale;
            paramMap.Set(YopConstants.LOCALE, this.locale);
        }

        public void setVersion(string version)
        {
            this.version = version;
            paramMap.Set(YopConstants.VERSION, this.version);
        }

        public void setMethod(string method)
        {
            this.method = method;
            paramMap.Set(YopConstants.METHOD, this.method);
        }

        public string getMethod()
        {
            return method;
        }

        public FormatType getFormat()
        {
            return format;
        }

        public string getLocale()
        {
            return locale;
        }

        public string getVersion()
        {
            return version;
        }

        public string getSignAlg()
        {
            return signAlg;
        }

        public void setSignAlg(string signAlg)
        {
            this.signAlg = signAlg;
        }

        public string getCustomerNo()
        {
            return customerNo;
        }

        public void setCustomerNo(string customerNo)
        {
            this.customerNo = customerNo;
            paramMap.Add(YopConstants.CUSTOMER_NO, this.customerNo);
        }

        public bool isEncrypt()
        {
            return encrypt;
        }

        public void setEncrypt(bool encrypt)
        {
            this.encrypt = encrypt;
        }

        public bool isSignRet()
        {
            return signRet;
        }

        public void setSignRet(bool signRet)
        {
            this.signRet = signRet;
            paramMap.Add(YopConstants.SIGN_RETURN, this.signRet.ToString().ToLower());
        }

        public bool IsRest()
        {
            return isRest;
        }

        public void setRest(bool isRest)
        {
            this.isRest = isRest;
        }

        public int getReadTimeout()
        {
            return readTimeout;
        }

        public void setReadTimeout(int readTimeout)
        {
            this.readTimeout = readTimeout;
        }

        public int getConnectTimeout()
        {
            return connectTimeout;
        }

        public void setConnectTimeout(int connectTimeout)
        {
            this.connectTimeout = connectTimeout;
        }

        public string getAppKey()
        {
            return appKey;
        }

        public string getSecretKey()
        {
            return secretKey;
        }

        public string getYopPublicKey()
        {
            return yopPublicKey;
        }

        public void setServerRoot(string serverRoot)
        {
            this.serverRoot = serverRoot;
        }

        public string getServerRoot()
        {
            if (StringUtils.isBlank(serverRoot))
            {
                serverRoot = YopConfig.getServerRoot();
            }
            return serverRoot;
        }

        public void encoding(string enType)
        {
            try
            {
                foreach (string key in paramMap.AllKeys)
                {
                    string[] values = paramMap.GetValues(key);
                    List<string> encoded = new List<string>(values.Length);
                    foreach (string value in values)
                    {
                        if (StringUtils.isBlank(value))
                        {
                            continue;
                        }

                        if (enType == "blowfish")
                            paramMap.Set(key, UrlEncode(value));
                        else
                            paramMap.Set(key, UrlEncode(UrlEncode(value)));
                    }
                }
            }
            catch (Exception e)
            {
                throw new SystemException();
            }
        }

        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                //if (HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).Length > 1)
                //{
                //    builder.Append(HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).ToUpper());
                //}
                //else
                //{
                //    builder.Append(c);
                //}
                if (Uri.EscapeDataString(c.ToString()).Length > 1)
                {
                    builder.Append(Uri.EscapeDataString(c.ToString()).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }

            }
            return builder.ToString();
        }

        public string getAbsoluteURL()
        {
            return absoluteURL;
        }

        public void setAbsoluteURL(string absoluteURL)
        {
            this.absoluteURL = absoluteURL;
        }

        /// <summary>
        /// 将参数转换成k=v拼接的形式
        /// </summary>
        /// <returns></returns>
        public string toQueryString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (string key in paramMap.AllKeys)
            {
                List<string> values = new List<string>();
                System.Diagnostics.Debug.WriteLine(paramMap[key]);

                string[] str = StringUtils.trimToNull(paramMap[key]) == null ? new string[] { "null" } : paramMap[key].Split(',');
                for (int i = 0; i < str.Length; i++)
                {
                    values.Add(str[i]);
                }


                foreach (string strValue in values)
                {
                    builder.Append(builder.Length == 0 ? "" : "&");
                    builder.Append(key);
                    builder.Append("=");
                    builder.Append(strValue);
                }
            }

            return builder.ToString();
        }


    }
}

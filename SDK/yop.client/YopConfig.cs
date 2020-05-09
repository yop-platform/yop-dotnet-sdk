using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.client
{
    public class YopConfig
    {
        private static string serverRoot = "https://openapi.yeepay.com/yop-center";

        private static string yosServerRoot = "https://yos.yeepay.com/yop-center";

        private static string sdkVersion = "3.2.17";

        private static string sdkLangs = "C#";

        /// <summary>
        /// 开放应用名
        /// </summary>
        private static string appKey;

        /// <summary>
        /// AES密钥，注册开放应用时获取的密钥<br>
        /// 对应使用AES算法加解密
        /// </summary>
        private static string aesSecretKey;

        /// <summary>
        /// Hmac密钥，即易宝商户入网后所得的HmacKey<br>
        /// 非开放应用身份发起调用时（即不提供appKey时）使用此密钥签名及报文加密<br>
        /// 对应使用Blowfish算法加解密
        /// </summary>
        private static string hmacSecretKey;

        /// <summary>
        /// 连接超时时间
        /// </summary>
        private static int connectTimeout = 30000;

        /// <summary>
        /// 读取返回结果超时
        /// </summary>
        private static int readTimeout = 60000;

        public static string getAppKey()
        {
            return appKey;
        }

        public static void setAppKey(string appKey)
        {
            YopConfig.appKey = appKey;
        }

        public static string getAesSecretKey()
        {
            return aesSecretKey;
        }

        public static void setAesSecretKey(string aesSecretKey)
        {
            YopConfig.aesSecretKey = aesSecretKey;
        }

        public static string getHmacSecretKey()
        {
            return hmacSecretKey;
        }

        public static void setHmacSecretKey(string hmacSecretKey)
        {
            YopConfig.hmacSecretKey = hmacSecretKey;
        }

        public static string getServerRoot()
        {
            return serverRoot;
        }

        public static void setServerRoot(string serverRoot)
        {
            YopConfig.serverRoot = serverRoot;
        }

        public static string getYosServerRoot()
        {
            return yosServerRoot;
        }

        public static string getSdkVersion()
        {
            return sdkVersion;
        }

        public static string getSdkLangs()
        {
            return sdkLangs;
        }

        public static int getConnectTimeout()
        {
            return connectTimeout;
        }

        public static void setConnectTimeout(int connectTimeout)
        {
            YopConfig.connectTimeout = connectTimeout;
        }

        public static int getReadTimeout()
        {
            return readTimeout;
        }

        public static void setReadTimeout(int readTimeout)
        {
            YopConfig.readTimeout = readTimeout;
        }

        /**
         * 已设置appKey，默认使用AES密钥
         */
        public static string getSecret()
        {
            if (appKey != null && appKey.Trim().Length > 0)
            {
                return aesSecretKey;
            }
            else
            {
                return hmacSecretKey;
            }
        }

    }
}

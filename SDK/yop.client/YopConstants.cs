using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.client
{
    public class YopConstants
    {
        public const string CLIENT_VERSION = "2.0.1";

        public const string ENCODING = "UTF-8";

        public const string SUCCESS = "SUCCESS";

        public const string CALLBACK = "callback";

        /// <summary>
        /// 方法的默认参数名
        /// </summary>
        public const string METHOD = "method";

        /// <summary>
        /// 格式化默认参数名
        /// </summary>
        public const string FORMAT = "format";

        /// <summary>
        /// 本地化默认参数名
        /// </summary>
        public const string LOCALE = "locale";

        /// <summary>
        /// 会话id默认参数名
        /// </summary>
        public const string SESSION_ID = "sessionId";

        /// <summary>
        /// 应用键的默认参数名
        /// </summary>
        public const string APP_KEY = "appKey";

        /// <summary>
        /// 服务版本号的默认参数名
        /// </summary>
        public const string VERSION = "v";

        /// <summary>
        /// 签名的默认参数名
        /// </summary>
        public const string SIGN = "sign";

        /// <summary>
        /// 返回结果是否签名
        /// </summary>
        public const string SIGN_RETURN = "signRet";

        /// <summary>
        /// 商户编号
        /// </summary>
        public const string CUSTOMER_NO = "customerNo";

        /// <summary>
        /// 加密报文key
        /// </summary>
        public const string ENCRYPT = "encrypt";

        /// <summary>
        /// 时间戳
        /// </summary>
        public const string TIMESTAMP = "ts";

        /// <summary>
        /// 保护参数
        /// </summary>
        public static readonly string[] PROTECTED_KEY = { APP_KEY, VERSION, SIGN,
        METHOD, FORMAT, LOCALE, SESSION_ID, CUSTOMER_NO, ENCRYPT,
        SIGN_RETURN, TIMESTAMP
    };

        public const string ALG_MD5 = "MD5";
        public const string ALG_AES = "AES";
        public const string ALG_SHA = "SHA";
        public const string ALG_SHA1 = "SHA1";
        public const string ALG_SHA256 = "SHA-256";

        /// <summary>
        /// 判断是否为保护参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool isProtectedKey(string key)
        {
            foreach (string k in YopConstants.PROTECTED_KEY)
            {
                if (k.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

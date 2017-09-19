using SDK.common;
using SDK.yop.client;
using SDK.yop.utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.encrypt
{
    public class YopSignUtils
    {
        public static string sign(IDictionary<string, string> paramValues, IList<string> ignoreParamNames, string secret, string algName)
        {
            Assert.notNull(paramValues);
            Assert.notNull(secret);
            if (StringUtils.isBlank(algName))
            {
                algName = YopConstants.ALG_SHA1;
            }
            StringBuilder sb = new StringBuilder();
            List<string> paramNames = new List<string>(paramValues.Count);
            foreach (string key in paramValues.Keys)
            {
                paramNames.Add(key);
            }
            if (ignoreParamNames != null && ignoreParamNames.Count > 0)
            {
                foreach (string ignoreParamName in ignoreParamNames)
                {
                    paramNames.Remove(ignoreParamName);
                }
            }
            paramNames.Sort(delegate (string x, string y) { return string.CompareOrdinal(x, y); });

            sb.Append(secret);
            foreach (string paramName in paramNames)
            {
                if (StringUtils.isBlank(paramValues[paramName]))
                {
                    continue;
                }
                sb.Append(paramName).Append(paramValues[paramName]);
            }
            sb.Append(secret);

            return Digest.digest(sb.ToString(), algName);
        }

        /// <summary>
        /// 对业务结果签名进行校验
        /// </summary>
        /// <param name="result"></param>
        /// <param name="secret"></param>
        /// <param name="algName"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static bool isValidResult(string result, string secret, string algName, string sign)
        {
            Assert.notNull(secret);
            Assert.notNull(sign);
            if (StringUtils.isBlank(algName))
            {
                algName = YopConstants.ALG_SHA;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(secret);
            sb.Append(StringUtils.trimToEmpty(result));
            sb.Append(secret);
            string newSign = Digest.digest(sb.ToString(), algName);
            System.Diagnostics.Debug.WriteLine("本地签名：" + newSign + " | 服务端签名：" + sign);
            return StringUtils.equalsIgnoreCase(sign, newSign);
        }
    }
}

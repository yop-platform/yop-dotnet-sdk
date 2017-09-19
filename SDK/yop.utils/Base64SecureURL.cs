using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDK.yop.utils
{
    /// <summary>
    /// Base64编码转换安全的URL
    /// </summary>
    public static class Base64SecureURL
    {
        /// <summary>
        /// 字符串编码
        /// </summary>
        /// <param name="text">待编码的文本字符串</param>
        /// <returns>编码的文本字符串.</returns>
        public static string Encode(string text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            var base64 = Convert.ToBase64String(plainTextBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return base64;
        }

        /// <summary>
        /// 解码安全的URL文本字符串的Base64
        /// </summary>
        /// <param name="secureUrlBase64">Base64编码字符串安全的URL.</param>
        /// <returns>Cadena de texto decodificada.</returns>
        public static string Decode(string secureUrlBase64)
        {
            secureUrlBase64 = secureUrlBase64.Replace('-', '+').Replace('_', '/');
            switch (secureUrlBase64.Length % 4)
            {
                case 2:
                    secureUrlBase64 += "==";
                    break;
                case 3:
                    secureUrlBase64 += "=";
                    break;
            }
            var bytes = Convert.FromBase64String(secureUrlBase64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}

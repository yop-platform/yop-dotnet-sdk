using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.common
{
    public class StringUtils
    {
        public static int INDEX_NOT_FOUND = -1;

        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.hasLength(null) = false
        /// StringUtils.hasLength("") = false
        /// StringUtils.hasLength(" ") = true
        /// StringUtils.hasLength("Hello") = true
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool hasLength(string str)
        {
            return (str != null && str.Length > 0);
        }

        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.hasText(null) = false
        /// StringUtils.hasText("") = false
        /// StringUtils.hasText(" ") = false
        /// StringUtils.hasText("12345") = true
        /// StringUtils.hasText(" 12345 ") = true
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool hasText(string str)
        {
            if (!hasLength(str))
                return false;
            int strlen = str.Length;
            for (int i = 0; i < strlen; i++)
            {
                if (!char.IsWhiteSpace(str, i))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.hasText(null) = true
        /// StringUtils.hasText("") = true
        /// StringUtils.hasText(" ") = true
        /// StringUtils.hasText("12345") = false
        /// StringUtils.hasText(" 12345 ") = false
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isBlank(string str)
        {
            return !hasText(str);
        }

        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.hasText(null) = false
        /// StringUtils.hasText("") = false
        /// StringUtils.hasText(" ") = false
        /// StringUtils.hasText("12345") = true
        /// StringUtils.hasText(" 12345 ") = true
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isNotBlank(string str)
        {
            return hasText(str);
        }
        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.trim(null)          = null
        ///  StringUtils.trim("")            = ""
        ///  StringUtils.trim("     ")       = ""
        ///  StringUtils.trim("abc")         = "abc"
        ///  StringUtils.trim("    abc    ") = "abc"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string trim(string str)
        {
            return str == null ? null : str.Trim();
        }

        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.trimToNull(null)          = null
        /// StringUtils.trimToNull("")            = null
        /// StringUtils.trimToNull("     ")       = null
        /// StringUtils.trimToNull("abc")         = "abc"
        /// StringUtils.trimToNull("    abc    ") = "abc"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string trimToNull(string str)
        {
            string ts = trim(str);
            return isEmpty(ts) ? null : ts;
        }
        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.isEmpty(null)      = true
        /// StringUtils.isEmpty("")        = true
        /// StringUtils.isEmpty(" ")       = false
        /// StringUtils.isEmpty("bob")     = false
        /// StringUtils.isEmpty("  bob  ") = false
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static bool isEmpty(string cs)
        {
            return cs == null || cs.Length == 0;
        }
        /// <summary>
        /// 检查字符串是否为下列情况
        /// StringUtils.trimToEmpty(null)          = ""
        /// StringUtils.trimToEmpty("")            = ""
        /// StringUtils.trimToEmpty("     ")       = ""
        /// StringUtils.trimToEmpty("abc")         = "abc"
        /// StringUtils.trimToEmpty("    abc    ") = "abc" 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string trimToEmpty(string str)
        {
            return str == null ? "" : str.Trim();
        }
        /// <summary>
        /// StringUtils.equalsIgnoreCase(null, null)   = true
        /// StringUtils.equalsIgnoreCase(null, "abc")  = false
        /// StringUtils.equalsIgnoreCase("abc", null)  = false
        /// StringUtils.equalsIgnoreCase("abc", "abc") = true
        /// StringUtils.equalsIgnoreCase("abc", "ABC") = true
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static bool equalsIgnoreCase(string str1, string str2)
        {
            return str1 == null ? str2 == null : str1.Equals(str2, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// StringUtils.substringAfter(null, *)      = null
        /// StringUtils.substringAfter("", *)        = ""
        /// StringUtils.substringAfter(*, null)      = ""
        /// StringUtils.substringAfter("abc", "a")   = "bc"
        /// StringUtils.substringAfter("abcba", "b") = "cba"
        /// StringUtils.substringAfter("abc", "c")   = ""
        /// StringUtils.substringAfter("abc", "d")   = ""
        /// StringUtils.substringAfter("abc", "")    = "abc"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string substringAfter(string str, string separator)
        {
            if (isEmpty(str))
            {
                return str;
            }
            if (separator == null)
            {
                return "";
            }
            int pos = str.IndexOf(separator);
            if (pos == INDEX_NOT_FOUND)
            {
                return "";
            }
            return str.Substring(pos + separator.Length);
        }

        /// <summary>
        /// StringUtils.substringBeforeLast(null, *)      = null
        /// StringUtils.substringBeforeLast("", *)        = ""
        /// StringUtils.substringBeforeLast("abcba", "b") = "abc"
        /// StringUtils.substringBeforeLast("abc", "c")   = "ab"
        /// StringUtils.substringBeforeLast("a", "a")     = ""
        /// StringUtils.substringBeforeLast("a", "z")     = "a"
        /// StringUtils.substringBeforeLast("a", null)    = "a"
        /// StringUtils.substringBeforeLast("a", "")      = "a"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string substringBeforeLast(string str, string separator)
        {
            if (isEmpty(str) || isEmpty(separator))
            {
                return str;
            }
            int pos = str.LastIndexOf(separator);
            if (pos == INDEX_NOT_FOUND)
            {
                return str;
            }
            return str.Substring(0, pos);
        }

    }
}

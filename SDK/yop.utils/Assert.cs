using System;
using System.Collections.Generic;
using System.Text;
using SDK.common;

namespace SDK.yop.utils
{
    
    /// <summary>
    /// xu.hao
    /// </summary>
    public class Assert
    {
        public static void notNull(object obj, string message)
        {
            if (obj == null)
            {
                throw new ArgumentException(message);
            }
        }

        public static void notNull(object obj)
        {
            notNull(obj, "[Assertion failed] - this argument is required; it must not be null");
        }

        public static void hasText(string text, string message)
        {
            if (!StringUtils.hasText(text))
            {
                throw new ArgumentException(message);
            }
        }

    }
}

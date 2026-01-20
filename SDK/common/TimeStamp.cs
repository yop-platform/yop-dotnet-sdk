using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.common
{
    public class TimeStamp
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {

            //TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //return Convert.ToInt64(ts.TotalSeconds).ToString();
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}

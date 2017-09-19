using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SDK.yop.utils
{
    
    public class RegexUtil
    {
        public static string GetResResult(string reg, string target)
        {
            Regex regCondition = new Regex(reg, RegexOptions.IgnoreCase);
            MatchCollection mc = regCondition.Matches(target);
            if (mc.Count < 1)
            {
                return "";
            }
            return (mc[0].Result("$1").Trim() == "$1" ? "" : mc[0].Result("$1"));
        }
    }
}

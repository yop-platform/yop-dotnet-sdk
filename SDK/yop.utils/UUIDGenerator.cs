using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.utils
{
    public class UUIDGenerator
    {
        public static string generate()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString().Replace("-", "");
        }
    }
}

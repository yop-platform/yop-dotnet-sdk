using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.exception
{
    public class YopClientException : SystemException
    {
        public YopClientException(string message)
            : base(message)
        {

        }

        public YopClientException(string message, Exception cause)
            : base(message, cause)
        {

        }
    }
}

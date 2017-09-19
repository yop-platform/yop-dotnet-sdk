using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.error
{
    public class YopSubError
    {
        public string code { get; set; }

        public string message { get; set; }

        public YopSubError()
        {
        }

        public YopSubError(string code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}

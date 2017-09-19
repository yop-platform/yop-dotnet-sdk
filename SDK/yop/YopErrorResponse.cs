using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop
{
    public class YopErrorResponse
    {
        public string requestId { get; set; }

        private string code { get; set; }

        private string message { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.error
{
    public class YopError
    {
        public string code { get; set; }

        public string message { get; set; }

        public string solution { get; set; }

        private List<YopSubError> subErrs = new List<YopSubError>();
        public List<YopSubError> subErrors
        {
            get { return subErrs; }
            set { subErrs = value; }
        }

        public YopError()
        {
        }
    }
}

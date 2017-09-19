using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop
{
    public class YopResponseMetadata
    {
        public string yopRequestId { get; set; }

        public string yopContentSha256 { get; set; }

        public string contentDisposition { get; set; }

        public string transferEncoding { get; set; }

        public string contentEncoding { get; set; }

        private long contentLength = -1;
        public long ContentLength
        {
            get { return contentLength; }
            set { contentLength = value; }
        }

        public string contentMd5 { get; set; }

        public string contentRange { get; set; }

        public string contentType { get; set; }

        public DateTime date { get; set; }

        public string eTag { get; set; }

        public DateTime expires { get; set; }

        public DateTime lastModified { get; set; }

        public string server { get; set; }

        public string location { get; set; }

    }
}

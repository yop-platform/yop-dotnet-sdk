using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop
{
    using SDK.yop.exception;
    public class YopServiceException : YopClientException
    {
        public enum ErrorType
        {
            Client,
            Service,
            Unknown
        }

        /**
         * 请求服务的唯一标识号
         */
        public string requestId { get; set; }

        /**
         * 错误码
         */
        public string errorCode { get; set; }

        /**
         * 请求失败的责任方
         */
        private ErrorType errType = ErrorType.Unknown;
        public ErrorType errorType
        {
            get { return errType; }
            set { errType = value; }
        }
        /**
         * 错误消息
         */
        public string errorMessage { get; set; }

        /**
         * HTTP status code
         */
        public int statusCode { get; set; }

        public YopServiceException(string errorMessage)
            : base(null)
        {
            this.errorMessage = errorMessage;
        }

        public YopServiceException(Exception cause, string errorMessage)
            : base(null, cause)
        {
            this.errorMessage = errorMessage;
        }

        public override string Message
        {
            get
            {
                return this.errorMessage + " (Status Code: " + this.statusCode + "; Error Code: "
                        + this.errorCode + "; Request ID: " + this.requestId + ")";
            }
        }
    }
}

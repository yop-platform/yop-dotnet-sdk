using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop
{
    public enum ErrorCode
    {
        ACCESS_DENIED,//("AccessDenied"),

        INTERNAL_ERROR,//("InternalError"),
        INVALID_ACCESS_KEY_ID,//("InvalidAccessKeyId"),
        INVALID_HTTP_AUTH_HEADER,//("InvalidHTTPAuthHeader"),
        INVALID_HTTP_REQUEST,//("InvalidHTTPRequest"),
        INVALID_URI,//("InvalidURI"),
        INVALID_VERSION,//("InvalidVersion"),

        PRECONDITION_FAILED,//("PreconditionFailed"),
        REQUEST_EXPIRED,//("RequestExpired"),
        SIGNATURE_DOES_NOT_MATCH,//"SignatureDoesNotMatch"

        INAPPROPRIATE_JSON,//"InappropriateJSON"
        MALFORMED_JSON,//"MalformedJSON"

        OPT_IN_REQUIRED//"OptInRequired"
    }
}

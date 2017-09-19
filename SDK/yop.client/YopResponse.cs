using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.client
{
    using SDK.enums;
    using SDK.error;
    public class YopResponse
    {
        /**
         * 状态(SUCCESS/FAILURE)
         */
        public string state { get; set; }

        /**
         * 业务结果，非简单类型解析后为LinkedHashMap
         */
        public object result { get; set; }

        /**
         * 时间戳
         */
        public long ts { get; set; }

        /**
         * 结果签名，签名算法为Request指定算法，示例：SHA(<secret>stringResult<secret>)
         */
        public string sign { get; set; }

        /**
         * 错误信息
         */
        public YopError error { get; set; }

        /**
         * 字符串形式的业务结果，客户可自定义java类，使用YopMarshallerUtils.unmarshal做参数绑定
         */
        public string stringResult { get; set; }

        /**
         * 响应格式，冗余字段，跟Request的format相同，用于解析结果
         */
        public FormatType format { get; set; }

        /**
         * 业务结果签名是否合法，冗余字段
         */
        public bool validSign { get; set; }



        public bool isValidSign()
        {
            return validSign;
        }

        /**
         * 业务是否成功
         */
        public bool isSuccess()
        {
            return YopConstants.SUCCESS.Equals(state);
        }


        ///**
        // * 将业务结果转换为自定义对象（参数映射）
        // */
        //public <T> T unmarshal(Class<T> objectType)
        //{
        //  if (objectType != null && StringUtils.isNotBlank(stringResult))
        //  {
        //    return YopMarshallerUtils.unmarshal(stringResult, format,
        //        objectType);
        //  }
        //  return null;
        //}

        ///**
        // * 将业务结果转换为自定义对象（参数映射）
        // */
        //public JsonNode parse()
        //{
        //  if (StringUtils.isNotBlank(stringResult) && format == FormatType.json)
        //  {
        //    return YopMarshallerUtils.parse(stringResult);
        //  }
        //  return null;
        //}

        //public String toString()
        //{
        //  return ToStringBuilder.reflectionToString(this,
        //      ToStringStyle.SHORT_PREFIX_STYLE);
        //}
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Web;
using System.Linq;
using SDK.yop.client;

namespace SDK.yop.utils
{
    using client;

    public class HttpUtils
    {

        /// <summary>
        /// The Set of accepted and valid Url characters per RFC3986.
        /// Characters outside of this set will be encoded.
        /// </summary>
        public const string ValidUrlCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        /// <summary>
        /// The set of accepted and valid Url path characters per RFC3986.
        /// </summary>
        private static string ValidPathCharacters = DetermineValidPathCharacters();

        // Checks which path characters should not be encoded
        // This set will be different for .NET 4 and .NET 4.5, as
        // per http://msdn.microsoft.com/en-us/library/hh367887%28v=vs.110%29.aspx
        private static string DetermineValidPathCharacters()
        {
            const string basePathCharacters = "/:'()!*[]$";

            var sb = new StringBuilder();
            foreach (var c in basePathCharacters)
            {
                var escaped = Uri.EscapeUriString(c.ToString());
                if (escaped.Length == 1 && escaped[0] == c)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static HttpWebResponse PostAndGetHttpWebResponse(YopRequest yopRequest, string method, Hashtable headers = null)//(string targetUrl, string param, string method, int timeOut)
        {
            try
            {
                string targetUrl = yopRequest.getAbsoluteURL();//请求地址
                CookieContainer cc = new CookieContainer();
                string param = yopRequest.toQueryString();//请求参数
                byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(param);
                if (method.ToUpper() == "GET") targetUrl = targetUrl + (param.Length == 0 ? "" : ("?" + param));

                // 2.0 https证书无效解决方法
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                // 1.1 https证书无效解决方法
                //ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

                System.GC.Collect();//垃圾回收
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetUrl);
                request.Timeout = yopRequest.getReadTimeout();
                request.Method = method.ToUpper();

                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        string value = (string)headers[key];
                        request.Headers.Add(key, value);
                    }
                }

                request.Accept = "*/*";
                //request.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = ".NET/" + YopConfig.getSdkVersion();
                //request.Referer = refererUrl;
                request.CookieContainer = cc;
                request.ServicePoint.Expect100Continue = false;
                request.ServicePoint.ConnectionLimit = 10000;
                request.AllowAutoRedirect = true;
                request.ProtocolVersion = HttpVersion.Version10; //尝试解决基础链接已关闭问题
                request.KeepAlive = false;//尝试解决基础链接已关闭问题 有可能影响证书问题

                if (method.ToUpper() == "POST")
                {
                    request.ContentLength = method.ToUpper().Trim() == "POST" ? data.Length : 0;

                    Stream newStream = request.GetRequestStream();
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                }

                if (method.ToUpper() == "PUT")
                {
                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                    byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                    request.ContentType = "multipart/form-data; boundary=" + boundary;
                    request.Method = "POST";
                    request.KeepAlive = true;
                    request.Credentials = CredentialCache.DefaultCredentials;

                    Stream newStream = request.GetRequestStream();

                    //1.1 key/value
                    Dictionary<string, string> stringDict = new Dictionary<string, string>();
                    ArrayList aryParam = new ArrayList(param.Split('&'));
                    for (int i = 0; i < aryParam.Count; i++)
                    {
                        string a = (String)aryParam[i];             //遍历，并且赋值给了a
                        int n = a.IndexOf("=");
                        stringDict.Add(a.Substring(0, n), a.Substring(n + 1));

                    }

                    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    if (stringDict != null)
                    {
                        foreach (string key in stringDict.Keys)
                        {
                            if (key.Equals("_file")) { continue; }
                            newStream.Write(boundarybytes, 0, boundarybytes.Length);
                            string formitem = string.Format(formdataTemplate, key, stringDict[key]);
                            byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                            newStream.Write(formitembytes, 0, formitembytes.Length);
                        }
                    }

                    //1.2 file
                    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;

                    string filePath = yopRequest.getParamValue("_file"); 

                    newStream.Write(boundarybytes, 0, boundarybytes.Length);
                    string header = string.Format(headerTemplate, "_file", Path.GetFileName(filePath));
                    byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                    newStream.Write(headerbytes, 0, headerbytes.Length);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            newStream.Write(buffer, 0, bytesRead);
                        }
                    }
                   
                    //1.3 form end
                    newStream.Write(endbytes, 0, endbytes.Length);
                    newStream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Cookies = cc.GetCookies(request.RequestUri);
                return response;
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return (HttpWebResponse)ex.Response;
            }
        }

        /// <summary>
        /// 解决证书问题 不管证书有效否，直接返回有效
        /// </summary>
        internal class AcceptAllCertificatePolicy : ICertificatePolicy
        {
            public bool CheckValidationResult(ServicePoint sPoint, System.Security.Cryptography.X509Certificates.X509Certificate cert, WebRequest wRequest, int certProb)
            {
                return true;
            }
        }

        /// <summary>
        /// 解决证书问题 不管证书有效否，直接返回有效
        /// </summary>
        /// <param name= "sender" ></param>
        /// <param name= "certificate" ></param>
        /// <param name= "chain" ></param>
        /// <param name= "errors" ></param>
        /// <returns></returns>
        public static bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {
            return true;
        }

        public static HttpWebResponse PostFile(YopRequest yopRequest, IEnumerable<UploadFile> files, Hashtable headers = null)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(yopRequest.getAbsoluteURL());
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    string value = (string)headers[key];
                    request.Headers.Add(key, value);
                }
            }

            MemoryStream stream = new MemoryStream();

            byte[] line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            NameValueCollection values = yopRequest.getParams();
            //提交文本字段
            if (values != null)
            {
                string format = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
                foreach (string key in values.Keys)
                {
                    string s = string.Format(format, key, values[key]);
                    byte[] data = Encoding.UTF8.GetBytes(s);
                    stream.Write(data, 0, data.Length);
                }
                stream.Write(line, 0, line.Length);
            }

            line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            //提交文件
            if (files != null)
            {
                string fformat = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                foreach (UploadFile file in files)
                {
                    string s = string.Format(fformat, file.Name, file.Filename);
                    byte[] data = Encoding.UTF8.GetBytes(s);
                    stream.Write(data, 0, data.Length);

                    stream.Write(file.Data, 0, file.Data.Length);
                    stream.Write(line, 0, line.Length);
                }
            }

            request.ContentLength = stream.Length;

            Stream requestStream = request.GetRequestStream();

            stream.Position = 0L;
            stream.WriteTo(requestStream);
            stream.Close();

            requestStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        /**
        * @param $path
        * @return string
        */
        public static string getCanonicalURIPath(string path)
        {
            if (path == null)
            {
                return "/";
            }
            else if (path.StartsWith("/"))
            {
                return normalizePath(path);
            }
            else
            {
                return "/" + normalizePath(path);
            }
        }

        public static string normalizePath(string path)
        {
            StringBuilder sb = new StringBuilder();
            string unreservedChars = String.Concat(ValidUrlCharacters, ValidPathCharacters));
            foreach (char symbol in System.Text.Encoding.UTF8.GetBytes(value))
            {
                if (unreservedChars.IndexOf(symbol) != -1 )
                {
                    sb.Append(symbol);
                }
                else
                {
                    sb.Append(@"%" + Convert.ToString(symbol, 16));
                }
            }

            return (sb.ToString());
        }

        /**
        * @param $value
        * @return string
        */
        public static string normalize(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char symbol in System.Text.Encoding.UTF8.GetBytes(value))
            {
                if (ValidUrlCharacters.IndexOf(symbol) != -1 )
                {
                    sb.Append(symbol);
                }
                else
                {
                    sb.Append(@"%" + Convert.ToString(symbol, 16));
                }
            }

            return (sb.ToString());
        }

    }

}

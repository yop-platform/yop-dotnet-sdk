﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Web;
using System.Linq;
namespace SDK.yop.utils
{
    using client;

    public class HttpUtils
    {
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
                request.UserAgent = ".NET/3.2.19";
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

        public static HttpWebResponse PostFile(YopRequest yopRequest, IEnumerable<UploadFile> files)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(yopRequest.getAbsoluteURL());
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Headers.Add("Request-Id", UUIDGenerator.generate());
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

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
            return normalize(path).Replace("%2F", "/");
        }

        /**
        * @param $value
        * @return string
        */
        public static string normalize(string value)
        {

            return UrlEncode(value, System.Text.Encoding.GetEncoding("UTF-8"), true);

        }


        /// <summary>
        /// UrlEncode重写：小写转大写，特殊字符特换
        /// </summary>
        /// <param name="strSrc">原字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="bToUpper">是否转大写</param>
        /// <returns></returns>
        public static string UrlEncode(string strSrc, System.Text.Encoding encoding, bool bToUpper)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < strSrc.Length; i++)
            {
                string t = strSrc[i].ToString();
                //string k = HttpUtility.UrlEncode(t, encoding);
                string k = Uri.EscapeDataString(t);
                if (t == k)
                {
                    stringBuilder.Append(t);
                }
                else
                {
                    if (bToUpper)
                        stringBuilder.Append(k.ToUpper());
                    else
                        stringBuilder.Append(k);
                }
            }
            if (bToUpper)
                return stringBuilder.ToString().Replace("+", "%2B");
                //return stringBuilder.ToString().Replace("+", "%20");
            else
                return stringBuilder.ToString();
        }

        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }

        public static string Base64UrlEncode(string s)
        {
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        public static string Base64UrlDecode(string arg)
        {
            string s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default: throw new System.Exception(
                  "Illegal base64url string!");
            }
            return s; // Standard base64 decoder
        }





    }


}

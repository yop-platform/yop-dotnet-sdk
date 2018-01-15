using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Collections;

namespace RsaTest
{
    public partial class test5 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CookieContainer cookies = new CookieContainer();
            //add or use cookies  
            NameValueCollection querystring = new NameValueCollection();
            querystring["uname"] = "uname";
            querystring["passwd"] = "snake3";
            string uploadfile;// set to file to upload  
            uploadfile = "e:\\test.jpg";

            //everything except upload file and url can be left blank if needed  
            string outdata = UploadFileEx(uploadfile,
               "http://open.yeepay.com/yop-center", "/rest/v1.0/file/upload", "image/pjpeg",
               querystring, cookies); 



        }

        public static string UploadFileEx(string uploadfile, string url,
  string fileFormName, string contenttype, NameValueCollection querystring,
  CookieContainer cookies)
        {
            if ((fileFormName == null) ||
              (fileFormName.Length == 0))
            {
                fileFormName = "file";
            }

            if ((contenttype == null) ||
              (contenttype.Length == 0))
            {
                contenttype = "application/octet-stream";
            }
            string postdata;
            postdata = "?";
            if (querystring != null)
            {
                foreach (string key in querystring.Keys)
                {
                    postdata += key + "=" + querystring.Get(key) + "&";
                }
            }
            Uri uri = new Uri(url + postdata);

            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.CookieContainer = cookies;
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";

            // Build up the post message header  
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(fileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(Path.GetFileName(uploadfile));
            sb.Append("\"");
            sb.Append("");
            sb.Append("Content-Type: ");
            sb.Append(contenttype);
            sb.Append("");
            sb.Append("");

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            // Build the trailing boundary string as a byte array  
            // ensuring the boundary appears on a line by itself  
            byte[] boundaryBytes =
                Encoding.ASCII.GetBytes("--" + boundary + "");

            FileStream fileStream = new FileStream(uploadfile,
                          FileMode.Open, FileAccess.Read);
            long length = postHeaderBytes.Length + fileStream.Length +
                                boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // Write out our post header  
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // Write out the file contents  
            byte[] buffer = new Byte[checked((uint)Math.Min(4096,
                         (int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);

            // Write out the trailing boundary  
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse responce = webrequest.GetResponse();
            Stream s = responce.GetResponseStream();
            StreamReader sr = new StreamReader(s);

            return sr.ReadToEnd();
        }
    }
}
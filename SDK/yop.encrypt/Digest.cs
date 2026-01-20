using SDK.yop.client;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SDK.yop.encrypt
{
    public class Digest
    {
        /// <summary>
        /// 使用MD5算法计算摘要，并对结果进行hex转换
        /// </summary>
        /// <param name="input">源数据</param>
        /// <returns></returns>
        public static string md5Digest(string input)
        {
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytHash = md5.ComputeHash(bytValue);
                StringBuilder sb = new StringBuilder(bytHash.Length * 2);
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sb.Append(bytHash[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string SHA(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            byte[] data = SHA1.Create().ComputeHash(buffer);

            StringBuilder sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString().ToLower();
        }

        public static string SHA256(string input)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(input);
            byte[] data;
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                data = sha256.ComputeHash(SHA256Data);
            }

            StringBuilder sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString().ToLower();
        }

        public static string digest(string input, string algorithm)
        {
            string strContent = string.Empty;
            switch (algorithm.ToUpper().Trim())
            {
                case YopConstants.ALG_SHA256:
                    strContent = SHA256(input);
                    break;
            }
            return strContent;
        }
    }
}

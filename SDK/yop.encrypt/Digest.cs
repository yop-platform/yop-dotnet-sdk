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
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(input);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }

            return sTemp;
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
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] data = Sha256.ComputeHash(SHA256Data);

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
                case YopConstants.ALG_MD5:
                    strContent = md5Digest(input).ToLower();
                    break;
                case YopConstants.ALG_SHA:
                    strContent = SHA(input);
                    break;
                case YopConstants.ALG_SHA1:
                    strContent = SHA(input);
                    break;
                case YopConstants.ALG_SHA256:
                    strContent = SHA256(input);
                    break;
            }
            return strContent;
        }
    }
}

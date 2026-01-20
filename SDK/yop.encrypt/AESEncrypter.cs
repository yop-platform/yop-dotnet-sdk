using SDK.yop.utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SDK.yop.encrypt
{
    public class AESEncrypter
    {
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="encryptStr">明文</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string encrypt(string encryptStr, string key)
        {
            byte[] keyArray = Convert.FromBase64String(key);//UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(encryptStr);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyArray;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cTransform = aes.CreateEncryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
                }
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="decryptStr">密文</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string decrypt(string decryptStr, string key)
        {
            //byte[] keyArray = Convert.FromBase64String(key);//UTF8Encoding.UTF8.GetBytes(key);
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(decryptStr);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyArray;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.Zeros;
                using (ICryptoTransform cTransform = aes.CreateDecryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return UTF8Encoding.UTF8.GetString(resultArray, 0, resultArray.Length);
                }
            }
        }

        /// <summary>
        /// AES解密(128-ECB加密模式)
        /// </summary>
        /// <param name="toDecrypt">密文</param>
        /// <param name="key">秘钥(Base64String)</param>
        /// <returns></returns>
        public static string AESDecrypt(string toDecrypt, string key)
        {
                //byte[] keyArray = Convert.FromBase64String(key); //128bit
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyArray; //获取或设置对称算法的密钥
                    aes.Mode = CipherMode.ECB; //获取或设置对称算法的运算模式，必须设置为ECB  
                    aes.Padding = PaddingMode.PKCS7; //获取或设置对称算法中使用的填充模式，必须设置为PKCS7
                    aes.KeySize = 128;
                    aes.BlockSize = 128;
                    using (ICryptoTransform cTransform = aes.CreateDecryptor()) //用当前的 Key 属性和初始化向量 (IV) 创建对称解密器对象
                    {
                        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                        return UTF8Encoding.UTF8.GetString(resultArray);
                    }
                }
        }
    }
}

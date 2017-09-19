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
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
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
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.BlockSize = 128;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.Zeros;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray, 0, resultArray.Length);


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
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray; //获取或设置对称算法的密钥
                rDel.Mode = CipherMode.ECB; //获取或设置对称算法的运算模式，必须设置为ECB  
                rDel.Padding = PaddingMode.PKCS7; //获取或设置对称算法中使用的填充模式，必须设置为PKCS7
                rDel.KeySize = 128;
                rDel.BlockSize = 128;
                ICryptoTransform cTransform = rDel.CreateDecryptor(); //用当前的 Key 属性和初始化向量 (IV) 创建对称解密器对象
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}

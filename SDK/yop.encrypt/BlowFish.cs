using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.yop.encrypt
{
    public class BlowFish
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string data, string key)
        {
            string keystr = Digest.md5Digest(key);
            IBufferedCipher cipher = CipherUtilities.GetCipher("Blowfish/CFB8/NoPadding");
            KeyParameter keyParam = new KeyParameter(System.Text.Encoding.UTF8.GetBytes(keystr.ToLower().Substring(0, 16)));
            cipher.Init(true, new ParametersWithIV(keyParam, System.Text.Encoding.UTF8.GetBytes(keystr.ToLower().Substring(0, 8))));
            return Convert.ToBase64String(cipher.DoFinal(System.Text.Encoding.UTF8.GetBytes(data)));
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string data, string key)
        {
            string keystr = Digest.md5Digest(key);
            IBufferedCipher cipher = CipherUtilities.GetCipher("Blowfish/CFB8/NoPadding");
            KeyParameter keyParam = new KeyParameter(System.Text.Encoding.UTF8.GetBytes(keystr.ToLower().Substring(0, 16)));
            cipher.Init(false, new ParametersWithIV(keyParam, System.Text.Encoding.UTF8.GetBytes(keystr.ToLower().Substring(0, 8))));
            return System.Text.Encoding.UTF8.GetString(cipher.DoFinal(Convert.FromBase64String(data)));
        }
    }
}

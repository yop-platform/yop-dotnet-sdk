﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using SDK.yop.exception;
using SDK.yop.encrypt;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Digests;

namespace SDK.yop.utils
{
    public sealed class SHA1withRSA
    {
        /// <summary>
        /// pem SHA1withRSA签名
        /// </summary>
        /// <param name="content">待签名字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="input_charset">编码格式</param>
        /// <returns>签名后字符串</returns>
        public static string sign(string content, string privateKeyString, string input_charset)
        {
            AsymmetricKeyParameter privateKey = PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKeyString));
            ISigner signer = new RsaDigestSigner(new Sha256Digest());
            signer.Init(true, privateKey);
            byte[] data = Encoding.UTF8.GetBytes(content);
            signer.BlockUpdate(data, 0, data.Length);
            byte[] signature = signer.GenerateSignature();
            return Convert.ToBase64String(signature);
        }
        
        //HMAC SHA256
        public static Object HashHmac(string signatureString, string secretKey, bool raw_output = false)
        {
            var enc = Encoding.UTF8;
            HMACSHA256 hmac = new HMACSHA256(enc.GetBytes(secretKey));
            hmac.Initialize();

            byte[] buffer = enc.GetBytes(signatureString);
            if (raw_output)
            {
                return hmac.ComputeHash(buffer);
            }
            else
            {
                return BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();
            }
        }

        public static string ToHmacData(Dictionary<string, string> hmacData)
        {
            string content = "";
            foreach (KeyValuePair<string, string> kvp in hmacData)
            {
                content += kvp.Key + "=" + kvp.Value + "&";
            }
            content = content.Substring(0, content.Length - 1);
            return content;
        }

        /// <summary>
        /// pem格式公钥验签
        /// </summary>
        /// <param name="content">待验签字符串</param>
        /// <param name="signedString">签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="input_charset">编码格式</param>
        /// <returns>true(通过)，false(不通过)</returns>
        public static bool verify(string content, string signedString, string publicKey, string input_charset, string digestAlg=null)
        {
            bool result = false;
            byte[] Data = Encoding.GetEncoding(input_charset).GetBytes(content);
            byte[] data = Convert.FromBase64String(signedString);
            RSAParameters paraPub = ConvertFromPublicKey(publicKey);
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.ImportParameters(paraPub);

            if (digestAlg==null||!digestAlg.Equals("SHA512"))
            {
                using (var sh = SHA256.Create())
                {
                    result = rsaPub.VerifyData(Data, sh, data);
                    return result;
                }
            }else
            {
                using (var sh = SHA512.Create())
                {
                    result = rsaPub.VerifyData(Data, sh, data);
                    return result;
                }
            }
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string encrypt(string publickey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.ImportParameters(ConvertFromPublicKey(publickey));
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);
            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="resData">加密字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="input_charset">编码格式</param>
        /// <returns>明文</returns>
        public static string decryptData(string resData, string privateKey, string input_charset)
        {
            byte[] DataToDecrypt = Convert.FromBase64String(resData);
            string result = "";
            for (int j = 0; j < DataToDecrypt.Length / 128; j++)
            {
                byte[] buf = new byte[128];
                for (int i = 0; i < 128; i++)
                {
                    buf[i] = DataToDecrypt[i + 128 * j];
                }
                result += decrypt(buf, privateKey, input_charset);
            }
            return result;
        }

        #region 内部方法
        private static string decrypt(byte[] data, string privateKey, string input_charset)
        {
            string result = "";
            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
            using (var sh = SHA1.Create())
            {
                byte[] source = rsa.Decrypt(data, false);
                char[] asciiChars = new char[Encoding.GetEncoding(input_charset).GetCharCount(source, 0, source.Length)];
                Encoding.GetEncoding(input_charset).GetChars(source, 0, source.Length, asciiChars, 0);
                result = new string(asciiChars);
                //result = ASCIIEncoding.ASCII.GetString(source);
                return result;
            }
        }

        private static RSACryptoServiceProvider DecodePemPrivateKey(String pemstr)
        {
            RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(Convert.FromBase64String(pemstr));
            return rsa;
        }

        private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem); //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;
                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0001)
                    return null;
                seq = binr.ReadBytes(15);	//read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x04)	//expect an Octet string 
                    return null;
                bt = binr.ReadByte();	//read next byte, or next 2 bytes is 0x81 or 0x82; otherwise bt is the byte count
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                        binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key
                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Dispose(); }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;
            // --------- Set up stream to decode the asn.1 encoded RSA private key ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem); //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte(); //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16(); //advance 2 bytes
                else
                    return null;
                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102) //version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;

                //------ all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);
                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                CspParameters CspParameters = new CspParameters();
                CspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024, CspParameters);
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch
            {
                return null;
            }
            finally
            {
                binr.Dispose();
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)	//expect integer
                return 0;
            bt = binr.ReadByte();
            if (bt == 0x81)
                count = binr.ReadByte();	// data size in next byte
            else
                if (bt == 0x82)
                {
                    highbyte = binr.ReadByte(); // data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt; // we already have the data size
                }
            while (binr.ReadByte() == 0x00)
            {	//remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);	//last ReadByte wasn‘t a removed zero, so back up a byte
            return count;
        }
        #endregion

        #region 解析.net 生成的Pem
        private static RSAParameters ConvertFromPublicKey(string pemFileConent)
        {
            if (string.IsNullOrEmpty(pemFileConent))
            {
                throw new ArgumentNullException("pemFileConent", "This arg cann‘t be empty.");
            }
            pemFileConent = pemFileConent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            bool keySize1024 = (keyData.Length == 162);
            bool keySize2048 = (keyData.Length == 294);
            if (!(keySize1024 || keySize2048))
            {
                throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
            }
            byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, (keySize1024 ? 29 : 33), pemModulus, 0, (keySize1024 ? 128 : 256));
            Array.Copy(keyData, (keySize1024 ? 159 : 291), pemPublicExponent, 0, 3);
            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            return para;
        }

        /// <summary>
        /// 将pem格式私钥(1024 or 2048)转换为RSAParameters
        /// </summary>
        /// <param name="pemFileConent">pem私钥内容</param>
        /// <returns>转换得到的RSAParamenters</returns>
        private static RSAParameters ConvertFromPrivateKey(string pemFileConent)
        {
            if (string.IsNullOrEmpty(pemFileConent))
            {
                throw new ArgumentNullException("pemFileConent", "This arg cann‘t be empty.");
            }
            pemFileConent = pemFileConent.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            bool keySize1024 = (keyData.Length == 609 || keyData.Length == 610);
            bool keySize2048 = (keyData.Length == 1190 || keyData.Length == 1192);
            if (!(keySize1024 || keySize2048))
            {
                throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
            }
            int index = (keySize1024 ? 11 : 12);
            byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemModulus, 0, pemModulus.Length);
            index += pemModulus.Length;
            index += 2;
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);
            index += 3;
            index += 4;
            if ((int)keyData[index] == 0)
            {
                index++;
            }
            byte[] pemPrivateExponent = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemPrivateExponent, 0, pemPrivateExponent.Length);
            index += pemPrivateExponent.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemPrime1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime1, 0, pemPrime1.Length);
            index += pemPrime1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemPrime2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime2, 0, pemPrime2.Length);
            index += pemPrime2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemExponent1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent1, 0, pemExponent1.Length);
            index += pemExponent1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemExponent2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent2, 0, pemExponent2.Length);
            index += pemExponent2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemCoefficient = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemCoefficient, 0, pemCoefficient.Length);
            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            para.D = pemPrivateExponent;
            para.P = pemPrime1;
            para.Q = pemPrime2;
            para.DP = pemExponent1;
            para.DQ = pemExponent2;
            para.InverseQ = pemCoefficient;
            return para;
        }
        #endregion

        public static string NoticeDecrypt(string source, string private_Key, string public_Key)
        {
 
            //分解参数
            string[] args = source.Split('$');

            if (args.Length != 4)
            {
                throw new YopClientException("source invalid :");
            }

            string encryptedRandomKeyToBase64 = args[0];
            string encryptedDataToBase64 = args[1];
            string symmetricEncryptAlg = args[2];
            string digestAlg = args[3];

            //用私钥对随机密钥进行解密
            byte[] randomKey = RsaAndAes.RSADecrypt(Base64SecureURL.Decode(encryptedRandomKeyToBase64), private_Key, "RSA");
            string encryptedData = Encoding.UTF8.GetString(RsaAndAes.AESDecrypt(Base64SecureURL.Decode(encryptedDataToBase64), randomKey));

            int index = encryptedData.LastIndexOf('$');

            string signToBase64 = encryptedData.Substring(index + 1);
            string sourceData = encryptedData.Substring(0, index);

            bool res = SHA1withRSA.verify(sourceData, Convert.ToBase64String(Base64SecureURL.Decode(signToBase64)), public_Key, "UTF-8", digestAlg);
            if (res)
            {
                return sourceData;
            }
            else
            {
                throw new YopClientException("verifySign fail!");
            }
        }

        public static string signRsa(string source, string private_Key)
        {
            string signToBase64 = SHA1withRSA.sign(source, private_Key, "UTF-8");

            signToBase64 = Base64SecureURL.Encode(signToBase64);

            signToBase64 += "$SHA256";

            return signToBase64;
        }

        private static RSA CreateRsaFromPrivateKey(string privateKey)
        {
            var privateKeyBits = System.Convert.FromBase64String(privateKey);
            var rsa = RSA.Create();
            var RSAparams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(RSAparams);
            return rsa;
        }

        //编码 
        public static string EncodeBase64(string code_type, string code)
        {
            string encode = "";
            byte[] bytes = Encoding.GetEncoding(code_type).GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }
        
        //解码 
        public static string DecodeBase64(string code_type, string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }

    }
}

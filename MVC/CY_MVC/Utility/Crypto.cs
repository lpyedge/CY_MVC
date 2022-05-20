using System;
using System.Security.Cryptography;
using System.Text;

namespace CY_MVC.Utility
{
    /// <summary>
    /// 不可逆加密辅助类
    /// </summary>
    public static class HASHCrypto
    {
        public enum DEncryptEnum
        {
            MD5 = 32,
            RIPEMD160 = 41,
            SHA1 = 40,
            SHA256 = 64,
            SHA384 = 96,
            SHA512 = 128
        }

        /// <summary>
        /// 创建不可逆加密类
        /// </summary>
        /// <param name="p_DEncryptType">加密类型</param>
        /// <param name="p_SecretKey">密钥</param>
        /// <param name="p_Encoding">编码格式</param>
        /// <returns>返回加密类HashAlgorithm</returns>
        public static HashAlgorithm Generate(DEncryptEnum p_DEncryptType = DEncryptEnum.SHA1, string p_SecretKey = "",
            Encoding p_Encoding = null)
        {
            p_Encoding = p_Encoding ?? Encoding.UTF8;
            HashAlgorithm ha = null;
            if (!string.IsNullOrEmpty(p_SecretKey))
            {
                HMAC hmac = null;
                switch (p_DEncryptType)
                {
                    case DEncryptEnum.MD5:
                        hmac = new HMACMD5();
                        break;

                    case DEncryptEnum.RIPEMD160:
                        hmac = new HMACRIPEMD160();
                        break;

                    case DEncryptEnum.SHA1:
                        hmac = new HMACSHA1();
                        break;

                    case DEncryptEnum.SHA256:
                        hmac = new HMACSHA256();
                        break;

                    case DEncryptEnum.SHA384:
                        hmac = new HMACSHA384();
                        break;

                    case DEncryptEnum.SHA512:
                        hmac = new HMACSHA512();
                        break;
                }
                hmac.Key = p_Encoding.GetBytes(p_SecretKey);
                ha = hmac;
            }
            else
            {
                switch (p_DEncryptType)
                {
                    case DEncryptEnum.MD5:
                        ha = new MD5CryptoServiceProvider();
                        break;

                    case DEncryptEnum.SHA1:
                        ha = new SHA1CryptoServiceProvider();
                        break;

                    case DEncryptEnum.RIPEMD160:
                        ha = new RIPEMD160Managed();
                        break;

                    case DEncryptEnum.SHA256:
                        ha = new SHA256Managed();
                        break;

                    case DEncryptEnum.SHA384:
                        ha = new SHA384Managed();
                        break;

                    case DEncryptEnum.SHA512:
                        ha = new SHA512Managed();
                        break;
                }
            }
            return ha;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="p_Provider">加密类HashAlgorithm</param>
        /// <param name="p_InputText">待加密字符串</param>
        /// <returns>返回大写字符串</returns>
        public static string Encrypt(this HashAlgorithm p_Provider, string p_InputText)
        {
            return Encrypt(p_Provider, Encoding.UTF8.GetBytes(p_InputText));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="p_Provider">加密类HashAlgorithm</param>
        /// <param name="p_InputBuff">待加密字节数组</param>
        /// <returns>返回大写字符串</returns>
        public static string Encrypt(this HashAlgorithm p_Provider, byte[] p_InputBuff)
        {
            var buff = Encrypt2Byte(p_Provider, p_InputBuff);
            return BitConverter.ToString(buff).Replace("-", string.Empty);
        }

        /// <summary>
        ///加密
        /// </summary>
        /// <param name="p_Provider">加密类HashAlgorithm</param>
        /// <param name="p_InputText">待加密字符串</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(this HashAlgorithm p_Provider, string p_InputText)
        {
            return Encrypt2Byte(p_Provider, Encoding.UTF8.GetBytes(p_InputText));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="p_Provider">加密类HashAlgorithm</param>
        /// <param name="p_InputBuff">待加密字节数组</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(this HashAlgorithm p_Provider, byte[] p_InputBuff)
        {
            var res = string.Empty;
            if (p_InputBuff != null)
            {
                return p_Provider.ComputeHash(p_InputBuff);
            }
            return new byte[0];
        }


        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="p_InputText">待加密字符串</param>
        /// <param name="p_EncryptEnum">加密类类型 DEncryptEnum</param>
        /// <param name="p_SecretKey">加密盐值密钥</param>
        /// <returns>返回大写字符串</returns>
        public static string Encrypt(string p_InputText, DEncryptEnum p_EncryptEnum, string p_SecretKey = "")
        {
            return Generate(p_EncryptEnum, p_SecretKey).Encrypt(p_InputText);
        }

        /// <summary>
        ///加密
        /// </summary>
        /// <param name="p_InputText">待加密字符串</param>
        /// <param name="p_EncryptEnum">加密类类型 DEncryptEnum</param>
        /// <param name="p_SecretKey">加密盐值密钥</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(string p_InputText, DEncryptEnum p_EncryptEnum, string p_SecretKey = "")
        {
            return Generate(p_EncryptEnum, p_SecretKey).Encrypt2Byte(Encoding.UTF8.GetBytes(p_InputText));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="p_InputBuff">待加密字节数组</param>
        /// <param name="p_EncryptEnum">加密类类型 DEncryptEnum</param>
        /// <param name="p_SecretKey">加密盐值密钥</param>
        /// <returns>返回大写字符串</returns>
        public static string Encrypt(byte[] p_InputBuff, DEncryptEnum p_EncryptEnum, string p_SecretKey = "")
        {
            return Generate(p_EncryptEnum, p_SecretKey).Encrypt(p_InputBuff);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="p_InputBuff">待加密字节数组</param>
        /// <param name="p_EncryptEnum">加密类类型 DEncryptEnum</param>
        /// <param name="p_SecretKey">加密盐值密钥</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(byte[] p_InputBuff, DEncryptEnum p_EncryptEnum, string p_SecretKey = "")
        {
            return Generate(p_EncryptEnum, p_SecretKey).Encrypt2Byte(p_InputBuff);
        }
    }

}
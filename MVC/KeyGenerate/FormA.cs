using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KeyGenerate
{
    public partial class FormA : Form
    {
        public FormA()
        {
            InitializeComponent();
        }

        private string GenerateKey()
        {
            var password = ((FormB)this.Owner).textBox1.Text;
            var passed = password == "CHUyangKEji520";
            var nokey = !((FormB)this.Owner).radioA.Checked;
            var key = new SecureString();
            if (!string.IsNullOrWhiteSpace(textBox1.Text.Trim()) && !string.IsNullOrWhiteSpace(textBox2.Text.Trim()))
            {
                var Domains =
                    textBox1.Text.Trim().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var DomainsCount = Domains.Count > 20 ? 20 : 40 - Domains.Count;

                using (var ha = HASHCrypto.Generate(HASHCrypto.DEncryptEnum.SHA1))
                {
                    var tempkey = ha.Encrypt(Domains.OrderBy(p => p.ToLowerInvariant())
                                 .Aggregate("", (current, item) => current + item));

                    var LibraryFile = ((FileInfo)textBox2.Tag);
                    if (nokey)
                    {
                        tempkey = ha.Encrypt(textBox2.Text + tempkey.ToUpperInvariant());
                    }
                    else
                    {
                        if (LibraryFile.Exists)
                        {
                            tempkey = ha.Encrypt(ha.Encrypt(File.ReadAllBytes(LibraryFile.FullName)) + tempkey.ToUpperInvariant());
                        }
                        else
                        {
                            tempkey = ha.Encrypt(LibraryFile.FullName.ToLowerInvariant() + DomainsCount);
                        }
                    }
                    tempkey = ha.Encrypt(tempkey.Substring(DomainsCount) + (passed ? "" : password));
                    tempkey = ha.Encrypt("CY_MVC" + tempkey.Remove(DomainsCount));

                    foreach (var item in tempkey)
                    {
                        key.AppendChar(item);
                    }
                }
            }
            string SecretKey;
            IntPtr bstr = Marshal.SecureStringToBSTR(key);
            try
            {
                SecretKey = Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
            return SecretKey;
        }

        private void FormA_DragDrop(object sender, DragEventArgs e)
        {
            System.Array filelist = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            if (filelist.Length == 1)
            {
                foreach (string item in filelist)
                {
                    FileInfo file = new FileInfo(item);
                    textBox2.Text = file.Name.Replace(file.Extension, "");
                    textBox2.Tag = file;

                    textBox3.Text = GenerateKey();
                }
            }
        }

        private void FormA_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = GenerateKey();
        }
    }


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
            return BitConverter.ToString(buff).Replace("-", "");
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
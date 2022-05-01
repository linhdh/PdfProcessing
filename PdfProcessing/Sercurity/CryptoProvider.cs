using System;
using System.Security.Cryptography;
using System.Text;

namespace PdfProcessing.Sercurity
{
    public class CryptoProvider
    {
        public static string CustomerKey = "CryptoProvider-Cap2019Ture";

        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            md5.ComputeHash(Encoding.ASCII.GetBytes(text));

            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string Encrypt(string toEncrypt, bool useHashing = true)
        {
            try
            {
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
                byte[] resultArray = Encrypt(toEncryptArray, useHashing);
                return ToHexString(Convert.ToBase64String(resultArray, 0, resultArray.Length));
            }
            catch
            {
                return null;
            }
        }
        public static string Decrypt(string toDecrypt, bool useHashing = true)
        {
            try
            {
                byte[] toDecryptArray = Convert.FromBase64String(FromHexString(toDecrypt));
                byte[] resultArray = Decrypt(toDecryptArray, useHashing);
                return Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                return null;
            }
        }
        private static byte[] Encrypt(byte[] toEncrypt, bool useHashing = true)
        {
            byte[] keyArray;
            byte[] toEncryptArray = toEncrypt;

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(CustomerKey));
                hashmd5.Clear();
            }
            else
                keyArray = Encoding.UTF8.GetBytes(CustomerKey);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return resultArray;
        }
        private static byte[] Decrypt(byte[] toDecrypt, bool useHashing = true)
        {
            byte[] keyArray;
            byte[] toDecryptArray = toDecrypt;

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(CustomerKey));
                hashmd5.Clear();
            }
            else
                keyArray = Encoding.UTF8.GetBytes(CustomerKey);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);

            tdes.Clear();
            return resultArray;
        }
        private static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.UTF8.GetBytes(str);

            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
        private static string FromHexString(string hexStr)
        {
            var bytes = new byte[hexStr.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}

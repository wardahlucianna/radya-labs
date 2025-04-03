using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BinusSchool.Workflow.FnWorkflow.Helper
{
    public class EncryptString
    {
        private static string stringKey = "New51S";

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <returns></returns>
        public static string Encrypt(string stringInputText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(stringInputText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(stringKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    stringInputText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return stringInputText;
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <param name="stringKey"></param>
        /// <returns></returns>
        public static string Encrypt(string stringInputText, string stringKey)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();
            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(stringInputText);
            byte[] Salt = Encoding.ASCII.GetBytes(stringKey.Length.ToString());
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(stringKey, Salt);
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(PlainText, 0, PlainText.Length);
            cryptoStream.FlushFinalBlock();
            byte[] CipherBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(CipherBytes);
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <returns></returns>
        public static string Decrypt(string stringInputText)
        {
            stringInputText = stringInputText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(stringInputText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(stringKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    stringInputText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return stringInputText;
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <param name="stringKey"></param>
        /// <returns></returns>
        public static string Decrypt(string stringInputText, string stringKey)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();
            stringInputText = stringInputText.Replace(" ", "+");
            byte[] EncryptedData = Convert.FromBase64String(stringInputText);
            byte[] Salt = Encoding.ASCII.GetBytes(stringKey.Length.ToString());
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(stringKey, Salt);
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream(EncryptedData);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
            byte[] PlainText = new byte[EncryptedData.Length];
            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);
        }
    }
}

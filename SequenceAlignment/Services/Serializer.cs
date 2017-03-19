using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace SequenceAlignment.Services
{
    static class Serializer
    {
        public static string EncryptString(string input, string Key)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(input), Key));
        }
        public static string EncryptByte(byte[] input, string Key)
        {
            return Convert.ToBase64String(Encrypt(input, Key));
        }
        public static string Decrypt(string input, string Key)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(input), Key));
        }
        private static byte[] Encrypt(byte[] input, string Key)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Key, new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            CryptoStream cs = new CryptoStream(ms,
            aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.Close();
            return ms.ToArray();
        }
        private static byte[] Decrypt(byte[] input, string Key)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Key, new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            CryptoStream cs = new CryptoStream(ms,
            aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.Close();
            return ms.ToArray();
        }

        public static Tuple<string, string> ExtractSequenceFromFile(string FileContent)
        {
            string FirstSequence = FileContent.Substring(FileContent.IndexOf("First Sequence:") + "First Sequence:".Length, FileContent.IndexOf("Second Sequence:") - "Second Sequence:".Length).Trim();
            string SecondSequence = FileContent.Substring(FileContent.IndexOf("Second Sequence:") + "Second Sequence:".Length).Trim();
            return new Tuple<string, string>(FirstSequence, SecondSequence);
        }
    }
}

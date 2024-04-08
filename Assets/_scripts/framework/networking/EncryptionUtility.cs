using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace OTBT.Framework.Networking
{
    public class EncryptionUtility : MonoBehaviour
    {
        public static AesCryptoServiceProvider aesProvider { get; private set; }

        private static string EncryptDecrypt(string data, string key)
        {
            if (key.Equals("")) return data;

            string res = "";
            Debug.Log("ENCRYPT DECRYPT LENGTH: " + data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                res += (char)(data[i] ^ key[i % key.Length]);
            }
            return res;
        }

        public static AesCryptoServiceProvider GenerateAesCryptoProvider(string key, string iv)
        {
            if (aesProvider == null)
            {
                AesCryptoServiceProvider provider = new AesCryptoServiceProvider();
                provider.BlockSize = 128;
                provider.KeySize = 256;
                provider.Key = ASCIIEncoding.ASCII.GetBytes(key);
                provider.IV = ASCIIEncoding.ASCII.GetBytes(iv);
                provider.Mode = CipherMode.CBC;
                provider.Padding = PaddingMode.PKCS7;
                aesProvider = provider;
            }

            return aesProvider;
        }
        public static string Encrypt(string input, string key1, string iv)
        {
            return EncryptDecrypt(input, key1);

            /*
            AesCryptoServiceProvider aes = GenerateAesCryptoProvider(key1, iv);

            byte[] txtByteData = ASCIIEncoding.ASCII.GetBytes(input);
            ICryptoTransform data = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] result = data.TransformFinalBlock(txtByteData, 0, txtByteData.Length);

            return Convert.ToBase64String(result);*/
        }
        public static string Decrypt(string input, string key1, string iv)
        {
            return EncryptDecrypt(input, key1);

            /*
            AesCryptoServiceProvider aes = GenerateAesCryptoProvider(key1, iv);

            byte[] txtByteData = Convert.FromBase64String(input);
            ICryptoTransform data = aes.CreateDecryptor();

            byte[] result = data.TransformFinalBlock(txtByteData, 0, txtByteData.Length);

            return ASCIIEncoding.ASCII.GetString(result);*/
        }
    }
}

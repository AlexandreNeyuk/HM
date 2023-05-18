using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace HM
{
    internal class CriptoAES_Container
    {
        private const string KeyFileName = "HMCP.key"; // Имя криптографического контейнера

        /// <summary>
        /// генерация нового ключа безопастности для контейнера (с ним можно работать уже AES-256)  
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return aes.Key;
            }
        }

        /// <summary>
        /// сохранение ключа безопастности  в скрытый файл текущего пользователя
        /// </summary>
        /// <param name="key"></param>
        public static void SaveEncryptionKey(byte[] key)
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string keyFolderPath = Path.Combine(appDataFolderPath, "HM");

            // Создаем папку, если она не существует
            Directory.CreateDirectory(keyFolderPath);

            string keyFilePath = Path.Combine(keyFolderPath, KeyFileName);

            // Записываем ключ в файл
            File.WriteAllBytes(keyFilePath, key);

            // Скрываем файл
            File.SetAttributes(keyFilePath, File.GetAttributes(keyFilePath) | FileAttributes.Hidden);

        }

        /// <summary>
        /// чтение ключа безопстности из контейнера
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static byte[] RetrieveEncryptionKey()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string keyFolderPath = Path.Combine(appDataFolderPath, "HM");
            string keyFilePath = Path.Combine(keyFolderPath, KeyFileName);

            // Проверяем, существует ли файл с ключом
            if (!File.Exists(keyFilePath))
            {

                MessageBox.Show("Ключ безопасности не найден!");
                return null;

            }

            // Читаем ключ из файла
            return File.ReadAllBytes(keyFilePath);
        }


        /// <summary>
        /// Удаление папки с ключом 
        /// </summary>
        public static void DeleteKeyFolder()
        {
            string keyFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HM");

            if (Directory.Exists(keyFolderPath))
            {
                Directory.Delete(keyFolderPath, true);
                Console.WriteLine("Key folder deleted successfully.");
            }
            else
            {
                Console.WriteLine("Key folder does not exist.");
            }
        }


        /// <summary>
        /// Шифрование логина и пароля с помьщью ключа
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static string EncryptString(string input, byte[] encryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(input);
            using (var encryptor = Aes.Create())
            {
                encryptor.Key = encryptionKey;
                encryptor.GenerateIV();

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(encryptor.IV, 0, encryptor.IV.Length);

                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(clearBytes, 0, clearBytes.Length);
                        cryptoStream.Close();
                    }

                    byte[] encryptedBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// дешифрование логина и пароля с помощью ключа
        /// </summary>
        /// <param name="encryptedInput"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static string DecryptString(string encryptedInput, byte[] encryptionKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedInput);
            using (var encryptor = Aes.Create())
            {
                encryptor.Key = encryptionKey;

                byte[] iv = new byte[encryptor.BlockSize / 8];
                Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
                encryptor.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
                        cryptoStream.Close();
                    }

                    byte[] decryptedBytes = memoryStream.ToArray();
                    return Encoding.Unicode.GetString(decryptedBytes);
                }
            }
        }



    }
}

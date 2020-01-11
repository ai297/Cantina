using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис возвращает пароль в зашифрованном виде, который можно хранить в бд
    /// </summary>
    public class HashPasswordService : IHashService
    {
        private IConfiguration configuration;
        // В конструктор передаётся зависимость от сервиса конфигурации
        public HashPasswordService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// метод получает хэш пароля. если параметр salt не задан - генерирует рандомну "соль"
        /// </summary>
        public (string, string) GetHash(string password, string salt = null)
        {
            if (String.IsNullOrEmpty(password)) return (null, null);
            if (String.IsNullOrEmpty(salt)) salt = CreateSalt();
            
            string localKey = configuration["LOCAL_KEY"];    // дополнительная приписка к паролю, которая не хранится в бд

            // derive a 256-bit subkey (use HMACSHA1 with 1500 iterations)
            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: $"{password}{localKey}",
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 499,
                numBytesRequested: 256 / 8));

            return (hashedPassword, salt);
        }

        private string CreateSalt()
        {
            byte[] newSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(newSalt);
            }
            return Convert.ToBase64String(newSalt);
        }

        /// <summary>
        /// Метод используется для простого хэширования любой строки, для которой не требуется большая устойчивать к взлому
        /// </summary>
        /// <param name="str">строка, хэш которой нужно получить</param>
        /// <returns>строка хэша</returns>
        public string SimpleHash(string str)
        {
            byte[] salt = new byte[0];
            if (String.IsNullOrEmpty(str)) return "";
            else return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: str,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1,
                numBytesRequested: 128 / 8
                ));
        }
    }
}

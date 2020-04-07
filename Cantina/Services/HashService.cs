using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис возвращает пароль в зашифрованном виде, который можно хранить в бд
    /// </summary>
    public class HashService
    {
        string localKey;

        public HashService(IConfiguration configuration)
        {
            localKey = configuration.GetSection("LOCAL_KEY").Value;
        }

        public string Get256Hash(string value, string salt = null)
        {
            if (String.IsNullOrEmpty(value)) return null;
            if (String.IsNullOrEmpty(salt)) salt = localKey;
            int iterations = 499 + (29 - value.Length) ^ 4;

            // derive a 256-bit subkey (use HMACSHA1 with n iterations)
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: iterations,
                numBytesRequested: 256 / 8
            ));
        }

        public string Get128Hash(string value)
        {
            if (String.IsNullOrEmpty(value)) return null;
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(localKey),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 100,
                numBytesRequested: 128 / 8
            ));
        }

    }
}

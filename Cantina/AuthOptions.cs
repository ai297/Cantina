using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Cantina
{
    /// <summary>
    /// Настройки авторизации в одном месте
    /// </summary>
    public static class AuthOptions
    {
        public const string Issuer = "CantinaApiServer";    // издатель токена
        public const int TokenLifetime = 15;                // время жизни токена авторизации, мин.
        public const int RefreshLifetime = 48;              // время жизни рефреш-токена, ч.

        public const string ClaimID = "uid";                // для записи id юзера
        public const string ClaimUA = "ua";                 // маркер для рефреш-токена

        // метод возвращает ключ шифрования для генерации токенов доступа.
        public static SymmetricSecurityKey GetSymmetricSecurityKey(IConfiguration configuration)
        {
            string key = configuration["SECURITY_KEY"];
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        // алгоритм шифрования ключа
        public static string SecurityAlgorithm { get; } = SecurityAlgorithms.HmacSha256;
    }
}

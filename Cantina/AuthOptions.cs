using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Cantina
{
    /// <summary>
    /// Настройки авторизации в одном месте
    /// </summary>
    public static class AuthOptions
    {
        public const string Issuer = "CantinaApiServer";    // издатель токена
        public const int TokenLifetime = 4;                 // время жизни токена авторизации, ч

        /// <summary>
        /// Константы для названия клэймов
        /// </summary>
        public static class Claims
        {
            public const string ID = "uid";                 // для записи id юзера
            public const string Email = "eml";              // для e-mail'a
            public const string Role = "ut";                // для тип юзера (роли)
            public const string UserAgent = "ua";           // для записи юзер-агента
        }

        /// <summary>
        /// Константы для названий политик авторизации
        /// </summary>
        public static class AuthPolicy
        {
            public const string RequireAdminRole = "RequireAdminRole";
            public const string RequireBotRole = "RequireBotRole";
        }

        /// <summary>
        /// Алгоритм шифрования ключа токена
        /// </summary>
        public static string SecurityAlgorithm { get => SecurityAlgorithms.HmacSha256; }
    }
}

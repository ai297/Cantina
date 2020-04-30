using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    public class AuthOptions
    {
        public int AuthTokenLifetime { get; set; } = 10;                        // время жизни токена авторизации, часов.
        public string TokenIssuer { get; set; } = "CantinaServer";              // издатель токена
        public int ActivationTokenLifetime { get; set; } = 10;                  // время жизни токена активации аккаунта, дней

        public string SecurityKey { get; set; }
        public string ConfirmKey { get; set; }
        public string LocalKey { get; set; }
    }
}

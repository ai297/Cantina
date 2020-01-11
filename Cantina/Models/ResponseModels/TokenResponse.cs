using System;

namespace Cantina.Models
{
    /// <summary>
    /// Модель ответа при успешной авторизации.
    /// </summary>
    public class TokenResponse
    {
        public int UserId { get; set; }
        
        public string AccessToken { get; set; }
        public DateTime AccessExpires { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshExpires { get; set; }
    }
}

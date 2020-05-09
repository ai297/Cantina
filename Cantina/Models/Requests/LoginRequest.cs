using System.ComponentModel.DataAnnotations;

namespace Cantina.Models.Requests
{
    /// <summary>
    /// Описание запроса на авторизацию
    /// </summary>
    public class LoginRequest
    {
        [EmailAddress]
        public string Email { get; set; }
        [Password]
        public string Password { get; set; }

        public bool LongLife { get; set; } = false;
    }
}

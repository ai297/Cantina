namespace Cantina.Models
{
    /// <summary>
    /// Описание запроса на авторизацию
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}

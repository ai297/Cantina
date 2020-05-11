
namespace Cantina.Services
{
    /// <summary>
    /// Конфигурация отправки email'ов
    /// </summary>
    public class EmailOptions
    {
        public string Server { get; set; }
        public int Port { get; set; } = 23;
        public bool SSLEnable { get; set; } = false;
        public string Email { get; set; }
        public string ChatName { get; set; }
        public string Password { get; set; }

        public EmailOptions() { }
    }
}

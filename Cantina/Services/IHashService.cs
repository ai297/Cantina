using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис, который выдаёт хэш сумму для заданной строки
    /// </summary>
    public interface IHashService
    {
        // хэш + соль
        public HashedPassword GetHash(string str, string salt = null);

        // простой хэш
        public string SimpleHash(string str);
    }
}

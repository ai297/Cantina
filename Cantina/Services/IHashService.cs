using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис, который выдаёт хэш сумму для заданной строки
    /// </summary>
    public interface IHashService
    {
        // хэш + соль
        public (string, string) GetHash(string password, string salt = null);

        // простой хэш
        public string SimpleHash(string str);
    }
}

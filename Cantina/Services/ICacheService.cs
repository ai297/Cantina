using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Интерфейс для сервиса кэширования
    /// </summary>
    /// <typeparam name="T">Модель, которую будем кэшировать</typeparam>
    public interface ICacheService<T> where T: class
    {
        public Task<T> Get(int id);

        public Task<bool> Add(T obj);

        public Task<bool> Update(T obj);

        public void AddToCache(T obj);
    }
}

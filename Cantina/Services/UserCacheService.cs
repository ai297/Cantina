using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис для кэширования информации о пользователях
    /// </summary>
    class UserCacheService : ICacheService<User>
    {
        private DataContext database;
        private IMemoryCache cache;
        
        public UserCacheService(DataContext context, IMemoryCache memoryCache)
        {
            database = context;     // подключаем сервис контекста базы данных
            cache = memoryCache;    // и сервис для кэширования
        }
        
        /// <summary>
        /// Метод добавляет нового юзера в базу данных и в кеш
        /// </summary>
        /// <param name="user">экземпляр User</param>
        /// <returns>получилось добавить в базу (true) или нет (false)</returns>
        public async Task<bool> Add(User user)
        {
            // добавляем юзера в базу данных
            database.Users.Add(user);
            var added = await database.SaveChangesAsync();
            // если хоть какой-то юзер в базу добавлен - добавляем его так же в кэш и возвращаем true
            if (added > 0)
            {
                AddToCache(user);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Метод извлекает юзера из кеша или базы данных по Id. Если юзер с заданным Id не найден - возвращает null
        /// </summary>
        /// <param name="id">Id юзера</param>
        /// <returns>Экземпляр User с заданным Id или null</returns>
        public async Task<User> Get(int id)
        {
            User user = null;
            // если в кеше есть юзер с заданным id - извлекаем его
            if(!cache.TryGetValue(id, out user))
            {
                // иначе получаем юзера из базы данных
                user = await database.Users.SingleOrDefaultAsync<User>(u => u.Id == id);
                // если юзер найден - сохраняем его в кеш
                if (user != null) AddToCache(user);
            }
            return user;
        }

        /// <summary>
        /// Метод обновляет информацию о юзере в базе и в кеше (в случае, если удалось обновить в базе)
        /// </summary>
        /// <param name="user">экземпляр существующего User</param>
        /// <returns>если удалось обновить возвращаем true, если нет - false</returns>
        public async Task<bool> Update(User user)
        {
            // если юзер не задан или его id равен 0 (не существует в базе данных) - ничего не делаем
            if (user == null || user.Id == 0) return false;
            // обновляем юзера в базе данных
            database.Users.Update(user);
            var updated = await database.SaveChangesAsync();
            // если удалось обновить в базе - обновляем в кеше / добавляем в кеш и возвращаем true
            if (updated > 0)
            {
                AddToCache(user);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// метод добавляет заданного юзера в кеш
        /// </summary>
        /// <param name="user">Существующий в бд экземпляр User</param>
        public void AddToCache(User user)
        {
            if(user != null && user.Id > 0)
            {
                cache.Set<User>(user.Id, user,
                    // кешируем на 30 минут
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
            }
        }
    }
}

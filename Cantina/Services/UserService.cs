using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис инкапсулирует логику работы с юзером
    /// </summary>
    public class UserService
    {
        private DataContext database;
        private IHashService hashService;
        private IMemoryCache memoryCache;
        
        public UserService(DataContext context, IHashService hashService, IMemoryCache cache)
        {
            this.database = context;            // подключаем сервис контекста базы данных
            this.hashService = hashService;     // сервивс хэширования
            this.memoryCache = cache;           // сервис кеширования
        }
        
        /// <summary>
        /// Метод добавляет нового юзера в базу данных
        /// </summary>
        public async Task<bool> AddUser(User user)
        {
            // добавляем юзера в базу данных
            database.Users.Add(user);
            var added = await database.SaveChangesAsync();
            // если хоть какой-то юзер в базу добавлен - добавляем его так же в кэш и возвращаем true
            if (added > 0)
            {
                addToCache(user);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Метод извлекает юзера из базы данных  по Id.
        /// </summary>
        public async Task<User> GetUser(int id)
        {
            User user = null;
            // пытаемся получить юзера из кеша, и если не выходит - получаем его из базы и добавляем в кеш
            if (!memoryCache.TryGetValue<User>(id, out user))
            {
                user = await database.Users.SingleOrDefaultAsync<User>(u => u.Id == id);
                addToCache(user);
            }
            return user;
        }
        /// <summary>
        /// Перегрузка ищет юзера по email
        /// </summary>
        public async Task<User> GetUser(string email)
        {
            var user = await database.Users.SingleOrDefaultAsync<User>(u => u.Email == email);
            addToCache(user);
            return user;
        }
        /// <summary>
        /// Находит юзера по email и возвращает только если верный пароль
        /// </summary>
        public async Task<User> GetUser(LoginRequest request)
        {
            // TODO: возможно нужны доп проверки на корректность request
            var user = await GetUser(request.Email);
            // если юзера нашли - проверяем пароль и если всё ок - возвращаем его, иначе возвращаем null
            if (user != null)
            {
                var userAuth = user.GetPasswordHash();
                if (userAuth.Item1 == hashService.GetHash(request.Password, userAuth.Item2).Item1 && user.Confirmed && user.Active)
                {
                    return user;
                }
            }
            return null;
        }


        /// <summary>
        /// Метод обновляет информацию о юзере в базе
        /// </summary>
        public async Task<bool> Update(User user)
        {
            // если юзер не задан или его id равен 0 (не существует в базе данных) - ничего не делаем
            if (user == null || user.Id == 0) return false;
            // обновляем юзера в базе данных
            database.Users.Update(user);
            var updated = await database.SaveChangesAsync();
            // если удалось обновить в базе - обновляем кеш и возвращаем true
            if (updated > 0)
            {
                addToCache(user);
                return true;
            }
            return false;
        }

        private void addToCache(User user)
        {
            if (user != null)
            {
                memoryCache.Set<User>(user.Id, user, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(90)  // время кеширования, в минутах
                });
            }
        }

    }
}

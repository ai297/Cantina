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
        private UsersHistoryService historyService;
        
        public UserService(DataContext context, IHashService hashService, IMemoryCache cache, UsersHistoryService historyService)
        {
            this.database = context;                // подключаем сервис контекста базы данных
            this.hashService = hashService;         // сервивс хэширования
            this.memoryCache = cache;               // сервис кеширования
            this.historyService = historyService;   // сервис логгирования активностей
        }
        
        /// <summary>
        /// Метод создаёт нового юзера на основе полученных данных.
        /// </summary>
        public async Task<bool> NewUser(RegisterRequest request)
        {
            // Создаём юзера.
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                Profile = new UserProfile
                {
                    Gender = request.Gender,
                    Location = request.Location
                }
            };
            // Хэшируем пароль.
            var hashedPassword = hashService.GetHash(request.Password);
            user.SetPasswordHash(hashedPassword);
            // Сохраняем в базу.
            var result = await addUser(user);
            // Добавляем запись о регистрации в историю активностей юзера
            if (result) historyService.NewActivityAsync(user, ActivityTypes.Register);
            return result;
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
        /// Метод обновляет информацию о юзере в базе
        /// </summary>
        public async void UpdateUserAsync(User user)
        {
            // если юзер не задан или его id равен 0 (не существует в базе данных) - ничего не делаем
            if (user == null || user.Id == 0) return;
            // обновляем юзера в базе данных
            database.Users.Update(user);
            var updated = await database.SaveChangesAsync();
            // если удалось обновить в базе - обновляем кеш
            if (updated > 0)
            {
                addToCache(user);
            }
        }

        // метод добавляет юзера в кеш
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
        // метод добавляет нового юзера в базу
        private async Task<bool> addUser(User user)
        {
            try
            {
                // добавляем юзера в базу данных
                database.Users.Add(user);
                var added = await database.SaveChangesAsync();
                // если хоть какой-то юзер в базу добавлен - добавляем его так же в кэш и возвращаем true
                if (added > 0)
                {
                    addToCache(user);
                    return true;
                } else return false;
            }
            catch
            {
                return false;
            }
        }
    }
}

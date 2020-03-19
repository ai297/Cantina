using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Cantina.Models;
using Cantina.Models.Requests;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис инкапсулирует логику работы с юзером
    /// </summary>
    public class UserService
    {
        DataContext database;
        HashService hashService;
        //IMemoryCache memoryCache;
        UsersHistoryService historyService;

        // сопоставление символов, считающихся похожими. только нижний регистр
        private static Dictionary<char, char> convertChars = new Dictionary<char, char>() {
            { 'а', 'a' },   // a-a, A-A
            { 'в', 'b' },   // В-B
            { 'е', 'e' },   // е-e, Е-E
            { 'ё', 'e' },   // ё-e, Ё-E
            { 'з', '3' },   // З-3
            { 'и', 'u' },   // и-u
            { 'й', 'u' },   // й-u
            { 'к', 'k' },   // К-K, к-k
            { 'м', 'm' },   // М-M
            { 'н', 'h' },   // Н-H
            { 'о', 'o' },   // о-o, О-O
            { 'п', 'n' },   // п-n
            { 'р', 'p' },   // р-p, Р-P
            { 'с', 'c' },   // с-c, С-C
            { 'т', 'm' },   // т-m
            { 't', 'т' },   // T-Т
            { 'у', 'y' },   // у-y, У-Y
            { 'х', 'x' },   // х-x, Х-X
            { 'ч', '4' },   // Ч-4
            { 'ь', 'b' },   // Ь-b
        };
        
        public UserService(DataContext context, HashService hashService, UsersHistoryService historyService)
        {
            this.database = context;                // подключаем сервис контекста базы данных
            this.hashService = hashService;         // сервис хэширования
            //this.memoryCache = cache;               // сервис кеширования
            this.historyService = historyService;   // сервис логгирования активностей
        }
        
        /// <summary>
        /// Метод создаёт нового юзера на основе полученных данных.
        /// </summary>
        public bool NewUser(RegisterRequest request)
        {
            // Создаём юзера.
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                Gender = request.Gender,
                Location = request.Location
            };
            // Хэшируем пароль.
            var hashedPassword = hashService.Get256Hash(request.Password);
            user.SetPassword(hashedPassword);
            // Сохраняем в базу.
            var result = addUser(user);
            // Добавляем запись о регистрации в историю активностей юзера
            if (result) _ = historyService.NewActivityAsync(user.Id, ActivityTypes.Register);
            return result;
        }

        /// <summary>
        /// Метод извлекает юзера из базы данных  по Id.
        /// </summary>
        public User GetUser(int id)
        {
            return database.Users.SingleOrDefault<User>(u => u.Id == id);
        }
        /// <summary>
        /// Перегрузка ищет юзера по email
        /// </summary>
        public User GetUser(string email)
        {
            return database.Users.SingleOrDefault<User>(u => u.Email == email);
        }
        /// <summary>
        /// Метод обновляет информацию о юзере в базе
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            // если юзер не задан или его id равен 0 (не существует в базе данных) - ничего не делаем
            if (user == null || user.Id == 0) return false;
            // обновляем юзера в базе данных
            database.Users.Update(user);
            var updated = await database.SaveChangesAsync();
            // если удалось обновить в базе - обновляем кеш
            if (updated > 0)
            {
                //addToCache(user);
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Метод проверяет имя на присутствие в таблице занятых имён
        /// </summary>
        public bool CheckNameForForbidden(string name)
        {
            var result = database.ForbiddenNames.Where(fn => fn.Name.Equals(GetNameModel(name))).ToArray().Count();
            return result > 0;
        }

        // метод добавляет юзера в кеш
        //private void addToCache(User user)
        //{
        //    if (user != null)
        //    {
        //        memoryCache.Set<User>(user.Id, user, new MemoryCacheEntryOptions
        //        {
        //            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(90)  // время кеширования, в минутах
        //        });
        //    }
        //}

        // метод добавляет нового юзера в базу
        private bool addUser(User user)
        {
            try
            {
                // добавляем юзера в базу данных
                database.Users.Add(user);
                database.ForbiddenNames.Add(new ForbiddenNames { Name = GetNameModel(user.Name), User = user });
                var added = database.SaveChanges();
                // если хоть какой-то юзер в базу добавлен - добавляем его так же в кэш и возвращаем true
                if (added > 0)
                {
                    return true;
                } else return false;
            }
            catch
            {
                return false;
            }
        }

        private string GetNameModel(string name)
        {
            name = name.ToLower();
            var fbname = new StringBuilder();
            char newCh;
            foreach(char ch in name)
            {
                if(ch != ' ')
                {
                    if (convertChars.TryGetValue(ch, out newCh))
                    {
                        fbname.Append(newCh);
                    }
                    else fbname.Append(ch);
                }
            }
            return fbname.ToString();
        }
    }
}

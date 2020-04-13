using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис инкапсулирует логику работы с юзером
    /// </summary>
    public class UserService
    {
        private readonly DataContext database;
        private readonly HashService hashService;

        // сопоставление символов, считающихся похожими. только нижний регистр
        private static readonly Dictionary<char, char> convertChars = new Dictionary<char, char>() {
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
            { ' ', '_' },   // -_
            { '\u00A0', '_' }, // -_
        };
        
        public UserService(DataContext context, HashService hashService)
        {
            this.database = context;                // подключаем сервис контекста базы данных
            this.hashService = hashService;         // сервис хэширования
        }
        
        /// <summary>
        /// Метод создаёт нового юзера на основе полученных данных.
        /// </summary>
        public async Task<User> AddUserAsync(string email, string name, string password)
        {
            // Создаём юзера.
            var user = new User
            {
                Email = email,
                Profile = new UserProfile
                {
                    Name = name
                }
            };
            // Хэшируем пароль.
            var hashedPassword = hashService.Get256Hash(password, email);
            user.SetPassword(hashedPassword);
            // Сохраняем в базу.
            try
            {
                // добавляем юзера в базу данных
                database.Users.Add(user);
                database.ForbiddenNames.Add(new ForbiddenNames { Name = GetNameModel(user.Profile.Name), User = user });
                await database.SaveChangesAsync();
                return user;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Метод извлекает юзера из базы данных  по Id.
        /// </summary>
        public User GetUser(int id)
        {
            return database.Users.Include(u => u.Profile).SingleOrDefault<User>(u => u.Id == id);
        }

        /// <summary>
        /// Ищет юзера по email
        /// </summary>
        public User GetUser(string email)
        {
            return database.Users.Include(u => u.Profile).SingleOrDefault<User>(u => u.Email.Equals(email));
        }

        /// <summary>
        /// Метод обновляет информацию о юзере в базе
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {

            if (user == null || user.Id == 0) return false;
            database.Users.Update(user);
            var updated = await database.SaveChangesAsync();
            return updated > 0;
        }

        /// <summary>
        /// Метод проверяет имя на присутствие в таблице занятых имён
        /// </summary>
        public bool CheckNameForForbidden(string name)
        {
            var result = database.ForbiddenNames.Where(fn => fn.Name.Equals(GetNameModel(name))).ToArray().Count();
            return result > 0;
        }

        /// <summary>
        /// Метод получает профиль юзера
        /// </summary>
        public UserProfile GetUserProfile(int userId)
        {
            return database.UserProfiles.SingleOrDefault<UserProfile>(up => up.User.Id == userId);
        }
        /// <summary>
        /// Метод обновляет профиль юзера
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(UserProfile profile)
        {
            if (profile == null || profile.UserId == 0) return false;
            var forbiddenName = database.ForbiddenNames.Where(fn => fn.UserId == profile.UserId).FirstOrDefault();
            forbiddenName.Name = GetNameModel(profile.Name);
            database.UserProfiles.Update(profile);
            database.ForbiddenNames.Update(forbiddenName);
            var updated = await database.SaveChangesAsync();
            return updated > 0;
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

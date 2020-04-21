using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Cantina.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис инкапсулирует логику работы с юзером
    /// </summary>
    public class UserService
    {
        private readonly DataContext _dataBase;
        private readonly HashService _hashService;
        private readonly IMemoryCache _memoryCache;
        
        // время хранения данных юзера в кеше
        private const int _cachTime = 60;
        
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

        public UserService(DataContext context, HashService hashService, IMemoryCache cache)
        {
            _dataBase = context;
            _hashService = hashService;
            _memoryCache = cache;
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
            var hashedPassword = _hashService.Get256Hash(password, email);
            user.SetPassword(hashedPassword);
            // Сохраняем в базу.
            try
            {
                // добавляем юзера в базу данных
                _dataBase.Users.Add(user);
                _dataBase.ForbiddenNames.Add(new ForbiddenNames { Name = GetNameModel(user.Profile.Name), User = user });
                _dataBase.History.Add(new UserHistory
                {
                    Date = DateTime.UtcNow,
                    Type = ActivityTypes.Register,
                    User = user,
                    Description = user.Profile.Name
                });
                await _dataBase.SaveChangesAsync();
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
            User user = null;
            if (!_memoryCache.TryGetValue<User>(id, out user))
            {
                user = _dataBase.Users.Include(u => u.Profile).SingleOrDefault<User>(u => u.Id == id);
                _memoryCache.Set<User>(user.Id, user, TimeSpan.FromMinutes(_cachTime));
            }
            return user;
        }

        /// <summary>
        /// Ищет юзера по email
        /// </summary>
        public User GetUser(string email)
        {
            var user = _dataBase.Users.Include(u => u.Profile).SingleOrDefault<User>(u => u.Email.Equals(email));
            _memoryCache.Set<User>(user.Id, user, TimeSpan.FromMinutes(_cachTime));
            return user;
        }

        /// <summary>
        /// Активация аккаунта
        /// </summary>
        public async Task<bool> Activate(User user)
        {
            user.Active = true;
            _dataBase.Update(user);
            _dataBase.History.Add(new UserHistory
            {
                Date = DateTime.UtcNow,
                Type = ActivityTypes.Activation,
                User = user
            });
            try
            {
                var added = await _dataBase.SaveChangesAsync() > 0;
                if (added) _memoryCache.Set<User>(user.Id, user, TimeSpan.FromMinutes(_cachTime));
                return added;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Метод обновляет информацию о юзере в базе
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {

            if (user == null || user.Id == 0) return false;
            _dataBase.Users.Update(user);
            var updated = await _dataBase.SaveChangesAsync() > 0;
            if (updated) _memoryCache.Set<User>(user.Id, user, TimeSpan.FromMinutes(_cachTime));
            return updated;
        }

        /// <summary>
        /// Метод проверяет имя на присутствие в таблице занятых имён
        /// </summary>
        public bool CheckNameForForbidden(string name)
        {
            var result = _dataBase.ForbiddenNames.Where(fn => fn.Name.Equals(GetNameModel(name))).ToArray().Count();
            return result > 0;
        }

        /// <summary>
        /// Метод получает профиль юзера
        /// </summary>
        public UserProfile GetUserProfile(int userId)
        {
            User user = null;
            if (!_memoryCache.TryGetValue<User>(userId, out user))
            {
                user = GetUser(userId);
                _memoryCache.Set<User>(userId, user, TimeSpan.FromMinutes(_cachTime));
                return user.Profile;
            }
            return (user == null) ? null : user.Profile;
        }

        /// <summary>
        /// Метод обновляет профиль юзера
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(UserProfile profile)
        {
            if (profile == null || profile.UserId == 0) return false;
            var forbiddenName = _dataBase.ForbiddenNames.Where(fn => fn.UserId == profile.UserId).FirstOrDefault();
            forbiddenName.Name = GetNameModel(profile.Name);
            _dataBase.UserProfiles.Update(profile);
            _dataBase.ForbiddenNames.Update(forbiddenName);
            var updated = await _dataBase.SaveChangesAsync() > 0;
            
            User user;
            if(updated && _memoryCache.TryGetValue<User>(profile.UserId, out user))
            {
                user.Profile = profile;
                _memoryCache.Set<User>(user.Id, user, TimeSpan.FromMinutes(_cachTime));
            }

            return updated;
        }

        private string GetNameModel(string name)
        {
            name = name.ToLower();
            var fbname = new StringBuilder();
            char newCh;
            foreach (char ch in name)
            {
                if (ch != ' ')
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

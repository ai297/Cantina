using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cantina.Models
{
    /// <summary>
    /// Базовая сущность юзера, хранит данные для авторизации.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор юзера.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// e-mail юзера, обязательно. Используется для авторизации вместо логина.
        /// </summary>
        [Required, EmailAddress]
        [MaxLength(64)]
        public string Email { get; set; }

        private string name;
        /// <summary>
        /// Никнейм, дополнительно обрабатывается для уменьшения вероятности совпадения
        /// </summary>        
        [NotMapped]
        [Required, Nickname]
        public string Name { get => name; set => name = NameConverter(value); }

        /// <summary>
        /// Подтверждён ли аккаунт. Если аккаунт не подтверждён - использовать его нельзя.
        /// </summary>
        public bool Confirmed { get; set; } = false;

        /// <summary>
        /// Активен лиаккаунт. если нет - использовать его нельзя.
        /// </summary>
        public bool Active { get; set; } = true;

        // Пароль, хранится в зашифрованном виде. Обязательное свойство.
        [Required]
        [MaxLength(128)]
        private string passwordHash;
        // "Соль" - приписка к паролю, что бы сложнее было подобрать.
        [Required]
        [MaxLength(64)]
        private string salt;

        /// <summary>
        /// Роль юзера
        /// </summary>
        public UserRoles Role { get; set; } = UserRoles.user;

        /// <summary>
        /// Профиль юзера.
        /// </summary>
        [Required]
        public UserProfile Profile { get; set; }

        /// <summary>
        /// Навигационное свойство ссылается на историю действий юзера.
        /// </summary>
        public virtual List<UserHistory> History { get; set; }

        /// <summary>
        /// Метод возвращает множество из 2х строк - хэш пароля и соль.
        /// </summary>
        public (string, string) GetPasswordHash()
        {
            return (passwordHash, salt);
        }
        /// <summary>
        /// Метод устанавливает значение хэша пароля и соль.
        /// </summary>
        public void SetPasswordHash(string hash, string salt)
        {
            this.passwordHash = hash;
            this.salt = salt;
        }

        private string NameConverter(string name)
        {
            /* Словарь для замены букв в именах
             * В качестве ключа идут буквы кириллицы
             * В качестве значения - аналогичные буквы латиницы.
             * В конечном варианте все имена в базу сохраняются с
             * английскими буквами вместо похожих на них русскими */
            var glossary = new Dictionary<char, char>()
            {
                { 'А', 'A' }, { 'а','a' },
                { 'В', 'B' },
                { 'Е', 'E' }, { 'е', 'e' },
                { 'К', 'K' },
                { 'М', 'M' },
                { 'Н', 'H' },
                { 'О', 'O' }, { 'о', 'o' },
                { 'Р', 'P' }, { 'р', 'p' },
                { 'С', 'C' }, { 'с', 'c' },
                { 'Т', 'T' },
                { 'у', 'y' },
                { 'Х', 'X' }, { 'х', 'x' }
            };
            var result = new StringBuilder();
            foreach (var c in name)
            {
                var currentChar = new char();
                if (glossary.TryGetValue(c, out currentChar)) result.Append(currentChar);
                else result.Append(c);
            }
            return result.ToString();
        }
    }
}
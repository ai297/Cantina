using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cantina.Models
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        /// <summary>
        /// E-mail юзера, обязательно. Используется для авторизации вместо логина.
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Email { get; set; }

        /// <summary>
        /// Подтверждён ли аккаунт. Сбрасывать на false при смене email'a.
        /// </summary>
        public bool Confirmed { get; set; } = false;

        /// <summary>
        /// Активен лиаккаунт. Если нет - использовать его нельзя.
        /// </summary>
        public bool Active { get; set; } = true;

        [Required, MaxLength(128)]
        private string password;
        /// <summary>
        /// Сеттер для пароля
        /// </summary>
        /// <param name="hash">Хэш пароля</param>
        public void SetPassword(string hash)
        {
            password = hash;
        }
        /// <summary>
        /// Сравнение хэша пароля
        /// </summary>
        /// <param name="value">хэш строка пароля для сравнения</param>
        /// <returns></returns>
        public bool PasswordEqual(string value)
        {
            return password.Equals(value);
        }

        /// <summary>
        /// Роль юзера
        /// </summary>
        public UserRoles Role { get; set; } = UserRoles.User;

        /// <summary>
        /// Дата окончания блокировки
        /// </summary>
        public DateTime? EndBlockDate { get; set; }

        /// <summary>
        /// Навигационное свойство на профиль юзера
        /// </summary>
        public UserProfile Profile { get; set; }

        /// <summary>
        /// Навигационное свойство ссылается на историю действий юзера.
        /// </summary>
        public virtual List<UserHistory> History { get; set; }

    }
}
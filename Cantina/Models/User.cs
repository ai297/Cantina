using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cantina.Models
{
    /// <summary>
    /// Базовая сущность юзера, хранит данные для авторизации
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор юзера
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// e-mail юзера, обязательно. Используется для авторизации вместо логина
        /// </summary>
        [Required, EmailAddress]
        [MaxLength(64)]
        public string Email { get; set; }

        /// <summary>
        /// Подтверждён ли аккаунт. Если аккаунт не подтверждён - использовать его нельзя
        /// </summary>
        public bool Confirmed { get; set; } = false;

        /// <summary>
        /// Активен лиаккаунт. если нет - использовать его нельзя
        /// </summary>
        public bool Active { get; set; } = true;

        // Пароль, хранится в зашифрованном виде. Обязательное свойство
        [Required]
        [MaxLength(128)]
        private string passwordHash;
        // "Соль" - приписка к паролю, что бы сложнее было подобрать
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
        /// Навигационное свойство ссылается на историю действий юзера
        /// </summary>
        public virtual List<UserHistory> History { get; set; }

        /// <summary>
        /// Метод возвращает множество из 2х строк - хэш пароля и соль
        /// </summary>
        public (string, string) GetPasswordHash()
        {
            return (passwordHash, salt);
        }
        /// <summary>
        /// Метод устанавливает значение хэша пароля и соль
        /// </summary>
        public void SetPasswordHash(string hash, string salt)
        {
            this.passwordHash = hash;
            this.salt = salt;
        }
    }
}
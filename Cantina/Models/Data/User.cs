using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Required, EmailAddress]
        [MaxLength(64)]
        public string Email { get; set; }

        /// <summary>
        /// Никнейм
        /// </summary>        
        [Required, Nickname]
        public string Name { get; set; }

        /// <summary>
        /// Подтверждён ли аккаунт. Сбрасывать на false при смене email'a.
        /// </summary>
        public bool Confirmed { get; set; } = false;

        /// <summary>
        /// Активен лиаккаунт. Если нет - использовать его нельзя.
        /// </summary>
        public bool Active { get; set; } = true;

        [Required]
        [MaxLength(128)]
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
        /// Пол, по умолчанию - не определившийся
        /// </summary>
        public Gender Gender { get; set; } = Gender.Uncertain;

        /// <summary>
        /// Откуда юзер
        /// </summary>
        [Location]
        [MaxLength(32)]
        public string Location { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Количество минут, проведённых онлайн
        /// </summary>
        public int OnlineTime { get; set; }

        /// <summary>
        /// Дата окончания блокировки
        /// </summary>
        public DateTime? EndBlockDate { get; set; }

        /// <summary>
        /// Настройки профиля.
        /// </summary>
        [NotMapped]
        public UserSettings Settings
        {
            get
            {
                if (!String.IsNullOrEmpty(settings)) return JsonSerializer.Deserialize<UserSettings>(settings);
                else return new UserSettings();
            }
            set
            {
                settings = JsonSerializer.Serialize<UserSettings>(value);
            }
        }
        private string settings; // Настройки юзера в сериализованном виде

        /// <summary>
        /// Навигационное свойство ссылается на историю действий юзера.
        /// </summary>
        public virtual List<UserHistory> History { get; set; }

    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Cantina.Models
{
    /// <summary>
    /// Модель запроса на обновление профиля
    /// </summary>
    public class UserProfile
    {
        public int UserId { get; set; }

        /// <summary>
        /// Никнейм юзера - должен быть уникальным
        /// </summary>
        [Nickname]
        [MaxLength(20)]
        public string Name { get; set; }
        /// <summary>
        /// Половая принадлежность
        /// </summary>
        public Gender Gender { get; set; } = Gender.Uncertain;
        /// <summary>
        /// Откуда
        /// </summary>
        [Location]
        [MaxLength(32)]
        public string Location { get; set; }
        /// <summary>
        /// Коротко о себе
        /// </summary>
        [MaxLength(255)]
        public string Description { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// Время, проведёное онлайн (в минутах).
        /// </summary>
        public int OnlineTime { get; set; }

        [NotMapped]
        public UserSettings Settings
        {
            get
            {
                if (!String.IsNullOrEmpty(settings)) return JsonSerializer.Deserialize<UserSettings>(settings);
                else return null;
            }
            set
            {
                settings = JsonSerializer.Serialize<UserSettings>(value);
            }
        }
        private string settings; // Настройки юзера в сериализованном виде

        public User User { get; set; }
    }
}

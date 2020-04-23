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


        //Переопределяем сравнение профилей для удобной проверки на наличие изменений
        public static bool operator ==(UserProfile p1, UserProfile p2)
        {
            return (p1.UserId == p2.UserId && p1.Name.Equals(p2.Name)
                    && p1.Location.Equals(p2.Location) && p1.Gender == p2.Gender
                    && p1.Description.Equals(p2.Description) && p1.Birthday == p2.Birthday
                    && p1.Settings.Equals(p2.Settings));
        }
        public static bool operator !=(UserProfile p1, UserProfile p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            return obj is UserProfile profile &&
                   UserId == profile.UserId;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, Name);
        }
    }
}

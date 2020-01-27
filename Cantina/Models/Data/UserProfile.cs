using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Cantina.Models
{
    /// <summary>
    /// Профиль пользователя
    /// </summary>
    [Owned]
    public class UserProfile
    {
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
        /// Стиль оформления сообщений.
        /// </summary>
        [NotMapped]
        public MessageStyle MessageStyle { 
            get
            {
                return JsonSerializer.Deserialize<MessageStyle>(messageStyle);
            }
            set
            {
                messageStyle = JsonSerializer.Serialize<MessageStyle>(value);
            }
        }
        private string messageStyle; // стиль сообщений в сериализованном виде

        /// <summary>
        /// Количество минут, проведённых онлайн
        /// </summary>
        public int OnlineTime { get; set; }
    }
}

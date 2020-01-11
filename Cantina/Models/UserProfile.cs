using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cantina.Models
{
    /// <summary>
    /// Профиль пользователя
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Никнейм, обязательное свойство
        /// </summary>
        [Required]
        [Nickname]
        [MaxLength(20), MinLength(4)]
        public string Name { get; set; }

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
        public MessageStyle MessageStyle { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        [Required]
        [Column(TypeName ="date")]
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Дата последнего визита
        /// </summary>
        [Column(TypeName ="date")]
        public DateTime? LastEnterDate { get; set; }

        /// <summary>
        /// Количество минут, проведённых онлайн
        /// </summary>
        public int OnlineTime { get; set; }
    }
}

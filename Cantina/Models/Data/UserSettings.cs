﻿namespace Cantina.Models
{
    /// <summary>
    /// Настройки профиля юзера
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Стиль отображения имени
        /// </summary>
        public FontStyle NameStyle { get; set; }
        /// <summary>
        /// Стиль отображения сообщения
        /// </summary>
        public FontStyle MessageStyle { get; set; }
    }
}

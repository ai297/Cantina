using System;
using System.Collections.Generic;

namespace Cantina.Models
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


        public override bool Equals(object obj)
        {
            return Equals(obj as UserSettings);
        }
        public bool Equals(UserSettings other)
        {
            return other != null &&
                   NameStyle.Equals(other.NameStyle) &&
                   MessageStyle.Equals(other.MessageStyle);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NameStyle, MessageStyle);
        }
    }
}

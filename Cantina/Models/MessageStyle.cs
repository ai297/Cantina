using System;
using Microsoft.EntityFrameworkCore;

namespace Cantina.Models
{
    /// <summary>
    /// Описывает оформление сообщения - цвет и шрифт никнейма и цвет и шрифт текста сообщения.
    /// </summary>
    public class MessageStyle
    {
        /// <summary>
        /// Стиль отображения никнейма.
        /// </summary>
        public FontStyle Name { get; set; }

        /// <summary>
        /// Стиль отображения сообщения.
        /// </summary>
        public FontStyle Message { get; set; }
    }
}

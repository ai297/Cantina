using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Структура хранит информацию о настройках визуального отображения шрифта.
    /// </summary>
    public struct FontStyle
    {
        /// <summary>
        /// Название шрифта в формате, подходящем для вставки в css.
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        /// Цвет шрифта
        /// </summary>
        public Color Color { get; set; }
    }
}

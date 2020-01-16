using System;
using System.Collections.Generic;
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
        /// Размер шрифта, один из предустановленных вариантов.
        /// </summary>
        public FontSizes Size { get; set; }

        /// <summary>
        /// Цвет шрифта в hex формате.
        /// </summary>
        public int Color { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Cantina.Models
{
    /// <summary>
    /// Стиль сообщения
    /// </summary>
    public class FontStyle
    {
        public RGBColor Color { get; set; }
        public string Font { get; set; }


        public override string ToString()
        {
            var fontFamily = (Font != null) ? $"font-family:{Font};" : "";
            var color = (Color.isNotABlack()) ? $"color:rgb({Color.R},{Color.G},{Color.B});" : "";
            return fontFamily + color;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FontStyle);
        }
        public bool Equals(FontStyle other)
        {
            return other != null &&
                   Color.Equals(other.Color) &&
                   Font == other.Font;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Color, Font);
        }
    }
}

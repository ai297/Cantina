using System;


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
    }
}

using System;

namespace Cantina.Models
{
    public struct RGBColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public RGBColor(byte r = 0, byte g = 0, byte b = 0)
        {
            R = r;
            G = g;
            B = b;
        }
        public RGBColor(byte[] rgb)
        {
            R = (rgb.Length > 0) ? rgb[0] : byte.MinValue;
            G = (rgb.Length > 1) ? rgb[1] : byte.MinValue;
            B = (rgb.Length > 2) ? rgb[2] : byte.MinValue;
        }

        public override string ToString()
        {
            return $"rgb({R},{G},{B});";
        }

        public bool isNotABlack()
        {
            return (R + G + B) > 0;
        }

        public override bool Equals(object obj)
        {
            return obj is RGBColor color && Equals(color);
        }

        public bool Equals(RGBColor other)
        {
            return R == other.R &&
                   G == other.G &&
                   B == other.B;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }
    }
}

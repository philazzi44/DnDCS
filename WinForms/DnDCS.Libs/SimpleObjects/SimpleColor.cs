
namespace DnDCS.Libs.SimpleObjects
{
    public class SimpleColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public SimpleColor()
        {
        }

        public SimpleColor(byte a, byte r, byte g, byte b)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public override string ToString()
        {
            return string.Format("A:{0}, R:{1}, G:{2}, B:{3}", A, R, G, B);
        }
    }
}

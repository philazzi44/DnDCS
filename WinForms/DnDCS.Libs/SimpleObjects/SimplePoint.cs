
namespace DnDCS.Libs.SimpleObjects
{
    public class SimplePoint
    {
        public static readonly SimplePoint Empty = new SimplePoint(0, 0);

        public int X { get; set; }
        public int Y { get; set; }

        public SimplePoint()
        {
        }

        public SimplePoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return string.Format("({0}/{1})", X, Y);
        }
    }
}

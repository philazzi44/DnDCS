
namespace DnDCS.Libs.SocketObjects
{
    public class DnDPoint
    {
        public static readonly DnDPoint Empty = new DnDPoint(0, 0);

        public int X { get; set; }
        public int Y { get; set; }

        public DnDPoint()
        {
        }

        public DnDPoint(int x, int y)
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

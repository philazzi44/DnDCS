
namespace DnDCS.Libs.SocketObjects
{
    public class SocketPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public SocketPoint()
        {
        }

        public SocketPoint(int x, int y)
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
